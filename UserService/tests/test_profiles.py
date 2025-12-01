import pytest
import uuid
from bson.objectid import ObjectId
from app.models import Profile 

BASE_URL = "/api/v1/profiles"

AUTH_ID_1 = str(uuid.uuid4())
AUTH_ID_2 = str(uuid.uuid4())

VALID_PROFILE = {
    "auth_user_id": AUTH_ID_1,
    "username": "TestUser",
    "full_name": "Juan Perez",
    "role": "student"
}

# --- 1. CREACION (POST) ---

def test_create_profile_success(client):
    response = client.post(f"{BASE_URL}/", json=VALID_PROFILE)
    assert response.status_code == 201
    data = response.json()
    assert data["username"] == "TestUser"
    assert data["auth_user_id"] == AUTH_ID_1
    assert "profile_id" in data

def test_create_profile_duplicate_username(client):
    client.post(f"{BASE_URL}/", json=VALID_PROFILE)
    
    duplicate_data = VALID_PROFILE.copy()
    duplicate_data["auth_user_id"] = str(uuid.uuid4()) 
    
    response = client.post(f"{BASE_URL}/", json=duplicate_data)
    assert response.status_code == 409
    assert "nombre de usuario" in response.json()["detail"]

def test_create_profile_duplicate_auth_id(client):
    client.post(f"{BASE_URL}/", json=VALID_PROFILE)
    
    duplicate_data = VALID_PROFILE.copy()
    duplicate_data["username"] = "OtherUser" 
    
    response = client.post(f"{BASE_URL}/", json=duplicate_data)
    assert response.status_code == 409
    assert "Este usuario ya tiene un perfil" in response.json()["detail"]

# --- 2. OBTENCION (GET) ---

def test_get_all_profiles_batch(client):
    # Insertamos 2 perfiles
    p1_data = VALID_PROFILE.copy()
    p1_data["auth_user_id"] = str(uuid.uuid4())
    p1_data["username"] = "BatchUser1"
    u1 = client.post(f"{BASE_URL}/", json=p1_data).json()
    
    p2_data = VALID_PROFILE.copy()
    p2_data["auth_user_id"] = str(uuid.uuid4())
    p2_data["username"] = "BatchUser2"
    u2 = client.post(f"{BASE_URL}/", json=p2_data).json()
    
    response = client.post(f"{BASE_URL}/batch", json={
        "profile_ids": [u1["profile_id"], u2["profile_id"]]
    })
    
    assert response.status_code == 200
    data = response.json()
    assert len(data) == 2
    usernames = [d["username"] for d in data]
    assert "BatchUser1" in usernames
    assert "BatchUser2" in usernames

def test_get_profile_by_auth_id_success(client):
    new_id = str(uuid.uuid4())
    profile_data = VALID_PROFILE.copy()
    profile_data["auth_user_id"] = new_id
    profile_data["username"] = "GetByIdUser"
    
    client.post(f"{BASE_URL}/", json=profile_data)
    
    response = client.get(f"{BASE_URL}/{new_id}")
    assert response.status_code == 200
    assert response.json()["full_name"] == "Juan Perez"

def test_get_profile_not_found(client):
    random_id = str(uuid.uuid4())
    response = client.get(f"{BASE_URL}/{random_id}")
    assert response.status_code == 404

# --- 3. ACTUALIZACION (PATCH) ---

def test_update_profile_success(client):
    uid = str(uuid.uuid4())
    p_data = VALID_PROFILE.copy()
    p_data["auth_user_id"] = uid
    p_data["username"] = "UpdateUser"
    client.post(f"{BASE_URL}/", json=p_data)
    
    update_payload = {
        "full_name": "Juan Actualizado",
        "bio": "Nueva biografia"
    }
    
    response = client.patch(f"{BASE_URL}/{uid}", json=update_payload)
    assert response.status_code == 200
    data = response.json()
    assert data["full_name"] == "Juan Actualizado"
    assert data["bio"] == "Nueva biografia"
    assert data["role"] == "student"

def test_update_profile_not_found(client):
    random_id = str(uuid.uuid4())
    response = client.patch(f"{BASE_URL}/{random_id}", json={"full_name": "Nadie"})
    assert response.status_code == 404

# --- 4. AVATAR (UPLOAD & GET) ---

def test_upload_avatar_success(client):
    uid = str(uuid.uuid4())
    p_data = VALID_PROFILE.copy()
    p_data["auth_user_id"] = uid
    p_data["username"] = "AvatarUser"
    client.post(f"{BASE_URL}/", json=p_data)
    
    files = {
        'file': ('avatar.png', b'fakeimagebytes', 'image/png')
    }
    
    response = client.post(f"{BASE_URL}/{uid}/avatar", files=files)
    
    assert response.status_code == 200
    data = response.json()
    assert data["profile_pic_id"] is not None 

def test_get_avatar_success(client):
    uid = str(uuid.uuid4())
    p_data = VALID_PROFILE.copy()
    p_data["auth_user_id"] = uid
    p_data["username"] = "GetAvatarUser"
    client.post(f"{BASE_URL}/", json=p_data)
    
    files = {'file': ('test.jpg', b'\x89PNG\r\nfakecontent', 'image/png')}
    client.post(f"{BASE_URL}/{uid}/avatar", files=files)
    
    response = client.get(f"{BASE_URL}/{uid}/avatar")
    assert response.status_code == 200
    assert response.headers["content-type"] == "image/png"
    assert response.content == b'\x89PNG\r\nfakecontent'

def test_get_avatar_user_not_found(client):
    random_id = str(uuid.uuid4())
    response = client.get(f"{BASE_URL}/{random_id}/avatar")
    assert response.status_code == 404
    assert "Perfil no encontrado" in response.json()["detail"]

def test_get_avatar_no_pic_associated(client):
    uid = str(uuid.uuid4())
    p_data = VALID_PROFILE.copy()
    p_data["auth_user_id"] = uid
    p_data["username"] = "NoPicUser"
    client.post(f"{BASE_URL}/", json=p_data)
    
    response = client.get(f"{BASE_URL}/{uid}/avatar")
    assert response.status_code == 404
    assert "Avatar no configurado" in response.json()["detail"]

def test_get_avatar_broken_link(client, db):
    """Caso borde: Postgres tiene un ID de foto, pero en Mongo no existe."""
    uid = str(uuid.uuid4())
    p_data = VALID_PROFILE.copy()
    p_data["auth_user_id"] = uid
    p_data["username"] = "BrokenLinkUser"
    client.post(f"{BASE_URL}/", json=p_data)
    
    user = db.query(Profile).filter_by(auth_user_id=uid).first()
    user.profile_pic_id = str(ObjectId()) 
    db.commit()
    
    response = client.get(f"{BASE_URL}/{uid}/avatar")
    assert response.status_code == 404
    assert "Imagen no encontrada" in response.json()["detail"]

# --- 5. BORRADO (DELETE) ---

def test_delete_profile_success(client):
    uid = str(uuid.uuid4())
    p_data = VALID_PROFILE.copy()
    p_data["auth_user_id"] = uid
    p_data["username"] = "DeleteUser"
    client.post(f"{BASE_URL}/", json=p_data)
    
    response = client.delete(f"{BASE_URL}/{uid}")
    assert response.status_code == 204 
    
    check = client.get(f"{BASE_URL}/{uid}")
    assert check.status_code == 404

def test_delete_profile_not_found(client):
    random_id = str(uuid.uuid4())
    response = client.delete(f"{BASE_URL}/{random_id}")
    assert response.status_code == 404