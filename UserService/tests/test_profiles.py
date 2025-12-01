import pytest
from bson.objectid import ObjectId

BASE_URL = "/api/v1/profiles"

VALID_PROFILE = {
    "auth_user_id": "uuid-1234-5678",
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
    assert data["auth_user_id"] == "uuid-1234-5678"
    assert "profile_id" in data

def test_create_profile_duplicate_username(client):
    client.post(f"{BASE_URL}/", json=VALID_PROFILE)
    
    duplicate_data = VALID_PROFILE.copy()
    duplicate_data["auth_user_id"] = "uuid-other-9999" 
    
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
    u1 = client.post(f"{BASE_URL}/", json=VALID_PROFILE).json()
    
    user2_data = VALID_PROFILE.copy()
    user2_data["auth_user_id"] = "uuid-2222"
    user2_data["username"] = "UserTwo"
    u2 = client.post(f"{BASE_URL}/", json=user2_data).json()
    
    response = client.post(f"{BASE_URL}/batch", json={
        "profile_ids": [u1["profile_id"], u2["profile_id"]]
    })
    
    assert response.status_code == 200
    data = response.json()
    assert len(data) == 2
    assert data[0]["username"] == "TestUser"
    assert data[1]["username"] == "UserTwo"

def test_get_profile_by_auth_id_success(client):
    client.post(f"{BASE_URL}/", json=VALID_PROFILE)
    
    response = client.get(f"{BASE_URL}/{VALID_PROFILE['auth_user_id']}")
    assert response.status_code == 200
    assert response.json()["full_name"] == "Juan Perez"

def test_get_profile_not_found(client):
    response = client.get(f"{BASE_URL}/uuid-inexistente")
    assert response.status_code == 404

# --- 3. ACTUALIZACION (PATCH) ---

def test_update_profile_success(client):
    client.post(f"{BASE_URL}/", json=VALID_PROFILE)
    
    update_payload = {
        "full_name": "Juan Actualizado",
        "bio": "Nueva biografia" 
    }
    
    response = client.patch(f"{BASE_URL}/{VALID_PROFILE['auth_user_id']}", json=update_payload)
    assert response.status_code == 200
    data = response.json()
    assert data["full_name"] == "Juan Actualizado"
    assert data["bio"] == "Nueva biografia"
    assert data["role"] == "student"

def test_update_profile_not_found(client):
    response = client.patch(f"{BASE_URL}/uuid-falso", json={"full_name": "Nadie"})
    assert response.status_code == 404

# --- 4. AVATAR (UPLOAD & GET) ---

def test_upload_avatar_success(client):
    client.post(f"{BASE_URL}/", json=VALID_PROFILE)
    
    files = {
        'file': ('avatar.png', b'fakeimagebytes', 'image/png')
    }
    
    auth_id = VALID_PROFILE['auth_user_id']
    response = client.post(f"{BASE_URL}/{auth_id}/avatar", files=files)
    
    assert response.status_code == 200
    data = response.json()
    assert data["profile_pic_id"] is not None 

def test_get_avatar_success(client):
    client.post(f"{BASE_URL}/", json=VALID_PROFILE)
    auth_id = VALID_PROFILE['auth_user_id']
    
    files = {'file': ('test.jpg', b'\x89PNG\r\nfakecontent', 'image/png')}
    client.post(f"{BASE_URL}/{auth_id}/avatar", files=files)
    
    response = client.get(f"{BASE_URL}/{auth_id}/avatar")
    assert response.status_code == 200
    assert response.headers["content-type"] == "image/png"
    assert response.content == b'\x89PNG\r\nfakecontent'

def test_get_avatar_user_not_found(client):
    response = client.get(f"{BASE_URL}/uuid-nadie/avatar")
    assert response.status_code == 404
    assert "Perfil no encontrado" in response.json()["detail"]

def test_get_avatar_no_pic_associated(client):
    client.post(f"{BASE_URL}/", json=VALID_PROFILE)
    
    response = client.get(f"{BASE_URL}/{VALID_PROFILE['auth_user_id']}/avatar")
    assert response.status_code == 404
    assert "Avatar no configurado" in response.json()["detail"]

def test_get_avatar_broken_link(client, db):
    client.post(f"{BASE_URL}/", json=VALID_PROFILE)
    
    from app.models import models 
    user = db.query(models.Profile).filter_by(auth_user_id=VALID_PROFILE['auth_user_id']).first()
    user.profile_pic_id = str(ObjectId()) 
    db.commit()
    
    response = client.get(f"{BASE_URL}/{VALID_PROFILE['auth_user_id']}/avatar")
    assert response.status_code == 404
    # Buscamos substring seguro sin tildes por si acaso
    assert "Imagen no encontrada" in response.json()["detail"]

# --- 5. BORRADO (DELETE) ---

def test_delete_profile_success(client):
    client.post(f"{BASE_URL}/", json=VALID_PROFILE)
    
    response = client.delete(f"{BASE_URL}/{VALID_PROFILE['auth_user_id']}")
    assert response.status_code == 204 
    
    check = client.get(f"{BASE_URL}/{VALID_PROFILE['auth_user_id']}")
    assert check.status_code == 404

def test_delete_profile_not_found(client):
    response = client.delete(f"{BASE_URL}/uuid-fantasma")
    assert response.status_code == 404