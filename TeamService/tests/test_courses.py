import pytest
import uuid

COURSES_URL = "/api/v1/courses"
MEMBERSHIPS_URL = "/api/v1/memberships"

PROFESSOR_UUID = str(uuid.uuid4())
STUDENT_UUID_1 = str(uuid.uuid4())
STUDENT_UUID_2 = str(uuid.uuid4())

# --- 1. CREACIÓN DE CURSOS ---

def test_create_course_success(client):
    payload = {
        "name": "Algoritmos Avanzados", 
        "description": "Curso de prueba",
        "professor_id": PROFESSOR_UUID
    }
    files = {'file': ('curso.png', b'fakebytes', 'image/png')}
    
    response = client.post(f"{COURSES_URL}/", data=payload, files=files)
    assert response.status_code == 201

def test_create_course_validation_error(client):
    payload = {
        "name": "Ab", 
        "professor_id": PROFESSOR_UUID
    }
    response = client.post(f"{COURSES_URL}/", data=payload)
    assert response.status_code == 422

# --- 2. UNIRSE A CURSO ---

def test_join_course_success(client):
    create_payload = {"name": "Historia Universal", "professor_id": PROFESSOR_UUID}
    files = {'file': ('test.jpg', b'img', 'image/jpeg')}
    
    course_res = client.post(f"{COURSES_URL}/", data=create_payload, files=files)
    assert course_res.status_code == 201
    access_code = course_res.json()["access_code"]
    
    join_payload = {"access_code": access_code, "user_id": STUDENT_UUID_1}
    response = client.post(f"{MEMBERSHIPS_URL}/join", json=join_payload)
    assert response.status_code == 201
    assert data["userid"] == STUDENT_UUID_1 

def test_join_course_invalid_code(client):
    join_payload = {"access_code": "CODIGO_FALSO", "user_id": STUDENT_UUID_1}
    response = client.post(f"{MEMBERSHIPS_URL}/join", json=join_payload)
    assert response.status_code == 404

def test_join_course_duplicate(client):
    create_payload = {"name": "Química Orgánica", "professor_id": PROFESSOR_UUID}
    files = {'file': ('test.jpg', b'img', 'image/jpeg')}
    course = client.post(f"{COURSES_URL}/", data=create_payload, files=files).json()
    
    payload = {"access_code": course["access_code"], "user_id": STUDENT_UUID_1}
    client.post(f"{MEMBERSHIPS_URL}/join", json=payload)
    
    response = client.post(f"{MEMBERSHIPS_URL}/join", json=payload)
    assert response.status_code == 409 

# --- 3. CONSULTAS (GET) ---

def test_get_all_courses(client):
    files = {'file': ('x.png', b'x', 'image/png')}
    client.post(f"{COURSES_URL}/", data={"name": "Curso Uno", "professor_id": str(uuid.uuid4())}, files=files)
    client.post(f"{COURSES_URL}/", data={"name": "Curso Dos", "professor_id": str(uuid.uuid4())}, files=files)
    
    response = client.get(f"{COURSES_URL}/") 
    assert response.status_code == 200
    assert len(response.json()) == 2

def test_get_professor_courses(client):
    files = {'file': ('x.png', b'x', 'image/png')}
    target_prof = str(uuid.uuid4())
    
    client.post(f"{COURSES_URL}/", data={"name": "Curso Profe A", "professor_id": target_prof}, files=files)
    client.post(f"{COURSES_URL}/", data={"name": "Curso Profe B", "professor_id": str(uuid.uuid4())}, files=files)
    
    response = client.get(f"{COURSES_URL}/professor/{target_prof}")
    assert response.status_code == 200
    data = response.json()
    assert len(data) == 1
    assert data[0]["name"] == "Curso Profe A"

def test_get_student_courses(client):
    files = {'file': ('x.png', b'x', 'image/png')}
    c1 = client.post(f"{COURSES_URL}/", data={"name": "Curso Alpha", "professor_id": str(uuid.uuid4())}, files=files).json()
    
    client.post(f"{MEMBERSHIPS_URL}/join", json={"access_code": c1["access_code"], "user_id": STUDENT_UUID_1})
    
    response = client.get(f"{COURSES_URL}/student/{STUDENT_UUID_1}")
    assert response.status_code == 200
    data = response.json()
    assert len(data) == 1
    assert data[0]["name"] == "Curso Alpha"

def test_get_students_in_course(client):
    files = {'file': ('x.png', b'x', 'image/png')}
    course = client.post(f"{COURSES_URL}/", data={"name": "Curso Grupal", "professor_id": PROFESSOR_UUID}, files=files).json()
    code = course["access_code"]
    
    client.post(f"{MEMBERSHIPS_URL}/join", json={"access_code": code, "user_id": STUDENT_UUID_1})
    client.post(f"{MEMBERSHIPS_URL}/join", json={"access_code": code, "user_id": STUDENT_UUID_2})
    
    response = client.get(f"{MEMBERSHIPS_URL}/course/{code}")
    assert response.status_code == 200
    data = response.json()
    assert len(data) == 2