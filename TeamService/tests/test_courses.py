import pytest
import uuid

COURSES_URL = "/api/v1/courses"
MEMBERSHIPS_URL = "/api/v1/memberships"

PROFESSOR_ID = str(uuid.uuid4())
STUDENT_ID = str(uuid.uuid4())
STUDENT_ID_2 = str(uuid.uuid4())

def test_create_course_success(client):
    payload = {
        "name": "Matemáticas Avanzadas",
        "description": "Curso de prueba",
        "professor_id": PROFESSOR_ID
    }
    files = {
        'file': ('curso.png', b'fakebytes', 'image/png')
    }
    
    response = client.post(f"{COURSES_URL}/", data=payload, files=files)
    assert response.status_code == 201
    
    data = response.json()
    assert data["name"] == "Matemáticas Avanzadas"
    assert data["professor_id"] == PROFESSOR_ID
    assert data["access_code"] is not None

def test_create_course_validation_error(client):
    payload = {
        "name": "Ab", 
        "professor_id": PROFESSOR_ID
    }
    response = client.post(f"{COURSES_URL}/", data=payload)
    assert response.status_code == 422

def test_join_course_success(client):
    create_payload = {"name": "Historia", "professor_id": PROFESSOR_ID}
    files = {'file': ('test.jpg', b'img', 'image/jpeg')}
    course_res = client.post(f"{COURSES_URL}/", data=create_payload, files=files)
    access_code = course_res.json()["access_code"]
    
    join_payload = {
        "access_code": access_code,
        "user_id": STUDENT_ID
    }
    response = client.post(f"{MEMBERSHIPS_URL}/join", json=join_payload)
    
    assert response.status_code == 201
    
    data = response.json()
    assert data["userid"] == STUDENT_ID

def test_join_course_invalid_code(client):
    join_payload = {
        "access_code": "CODIGO_FALSO",
        "user_id": STUDENT_ID
    }
    response = client.post(f"{MEMBERSHIPS_URL}/join", json=join_payload)
    
    assert response.status_code == 404

def test_join_course_duplicate(client):
    create_payload = {"name": "Química", "professor_id": PROFESSOR_ID}
    files = {'file': ('test.jpg', b'img', 'image/jpeg')}
    course = client.post(f"{COURSES_URL}/", data=create_payload, files=files).json()
    
    payload = {"access_code": course["access_code"], "user_id": STUDENT_ID}
    client.post(f"{MEMBERSHIPS_URL}/join", json=payload)
    
    response = client.post(f"{MEMBERSHIPS_URL}/join", json=payload)
    assert response.status_code == 409

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
    
    client.post(f"{COURSES_URL}/", data={"name": "Curso Profe", "professor_id": target_prof}, files=files)
    client.post(f"{COURSES_URL}/", data={"name": "Otro Curso", "professor_id": str(uuid.uuid4())}, files=files)
    
    response = client.get(f"{COURSES_URL}/professor/{target_prof}")
    assert response.status_code == 200
    data = response.json()
    assert len(data) == 1
    assert data[0]["professor_id"] == target_prof

def test_get_student_courses(client):
    files = {'file': ('x.png', b'x', 'image/png')}
    c1 = client.post(f"{COURSES_URL}/", data={"name": "Curso Alpha", "professor_id": PROFESSOR_ID}, files=files).json()
    
    client.post(f"{MEMBERSHIPS_URL}/join", json={"access_code": c1["access_code"], "user_id": STUDENT_ID})
    
    response = client.get(f"{COURSES_URL}/student/{STUDENT_ID}")
    assert response.status_code == 200
    data = response.json()
    assert len(data) == 1
    assert data[0]["name"] == "Curso Alpha"

def test_get_students_in_course(client):
    files = {'file': ('x.png', b'x', 'image/png')}
    course = client.post(f"{COURSES_URL}/", data={"name": "Curso Grupal", "professor_id": PROFESSOR_ID}, files=files).json()
    code = course["access_code"]
    
    client.post(f"{MEMBERSHIPS_URL}/join", json={"access_code": code, "user_id": STUDENT_ID})
    client.post(f"{MEMBERSHIPS_URL}/join", json={"access_code": code, "user_id": STUDENT_ID_2})
    
    response = client.get(f"{MEMBERSHIPS_URL}/course/{code}")
    assert response.status_code == 200
    data = response.json()
    assert len(data) == 2
    
    user_ids = [m["userid"] for m in data]
    assert STUDENT_ID in user_ids
    assert STUDENT_ID_2 in user_ids