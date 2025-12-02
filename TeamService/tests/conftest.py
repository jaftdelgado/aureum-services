import pytest
import os
from sqlalchemy import create_engine
from sqlalchemy.orm import sessionmaker
from fastapi.testclient import TestClient
from pymongo import MongoClient

from app.main import app
from app.database import Base, get_db
from app import mongo_db

DB_USER = os.getenv("POSTGRES_USER", "test_user")
DB_PASS = os.getenv("POSTGRES_PASSWORD", "test_pass")
DB_HOST = os.getenv("POSTGRES_HOST", "localhost")
DB_PORT = os.getenv("POSTGRES_PORT", "5432")
DB_NAME = os.getenv("POSTGRES_DB", "teams_test_db")

SQLALCHEMY_DATABASE_URL = f"postgresql://{DB_USER}:{DB_PASS}@{DB_HOST}:{DB_PORT}/{DB_NAME}"

engine = create_engine(SQLALCHEMY_DATABASE_URL)
TestingSessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

MONGO_URI = os.getenv("MONGO_URI", "mongodb://localhost:27017/teams_test_mongo")

@pytest.fixture(scope="function")
def db():
    """Crea tablas en Postgres al inicio y las borra al final."""
    from app.models import team, team_membership 
    
    Base.metadata.create_all(bind=engine)
    session = TestingSessionLocal()
    try:
        yield session
    finally:
        session.close()
        Base.metadata.drop_all(bind=engine)

@pytest.fixture(scope="function")
def mongo_client():
    """Conecta a Mongo y limpia la colección de imágenes al final."""
    client = MongoClient(MONGO_URI)
    db = client.get_database()
    
    class MockMongoDBClient:
        def get_collection(self):
            return db["team_images"]

    mongo_db.mongo_client = MockMongoDBClient()
    
    yield db
    
    db["team_images"].drop()
    client.close()

@pytest.fixture(scope="function")
def client(db, mongo_client):
    """Cliente HTTP."""
    def override_get_db():
        try:
            yield db
        finally:
            db.close()
    
    app.dependency_overrides[get_db] = override_get_db
    with TestClient(app) as c:
        yield c