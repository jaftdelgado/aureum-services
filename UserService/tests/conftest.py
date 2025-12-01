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
DB_NAME = os.getenv("POSTGRES_DB", "test_db")

SQLALCHEMY_DATABASE_URL = f"postgresql://{DB_USER}:{DB_PASS}@{DB_HOST}:{DB_PORT}/{DB_NAME}"

engine = create_engine(SQLALCHEMY_DATABASE_URL)
TestingSessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

MONGO_URI = os.getenv("MONGO_URI", "mongodb://localhost:27017/test_mongo_db")

@pytest.fixture(scope="function")
def db():
    Base.metadata.create_all(bind=engine)
    session = TestingSessionLocal()
    try:
        yield session
    finally:
        session.close()
        Base.metadata.drop_all(bind=engine)

@pytest.fixture(scope="function")
def mongo_client():
    real_client = MongoClient(MONGO_URI)
    test_db = real_client.get_database()
    class MockMongoDBClient:
        client = real_client
        db = test_db
        
        def get_collection(self):
            return self.db["profile_images"]

        def close(self):
            self.client.close()

    mock_instance = MockMongoDBClient()
    mongo_db.mongo_client = mock_instance
    yield db
    test_db["profile_images"].drop()
    real_client.close()

@pytest.fixture(scope="function")
def client(db, mongo_client):
    def override_get_db():
        try:
            yield db
        finally:
            db.close()
    
    app.dependency_overrides[get_db] = override_get_db
    with TestClient(app) as c:
        yield c