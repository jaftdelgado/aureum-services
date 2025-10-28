from sqlalchemy import create_engine
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker
import os

POSTGRES_USER = os.getenv("USUARIOS_DB_USER")
POSTGRES_PASSWORD = os.getenv("USUARIOS_DB_PASSWORD")
POSTGRES_DB = os.getenv("USUARIOS_DB_NAME", "usuarios") # Lee la variable, si no, usa "usuarios"
POSTGRES_HOST = os.getenv("USUARIOS_DB_HOST")
POSTGRES_PORT = os.getenv("POSTGRES_PORT", "5432")

SQLALCHEMY_DATABASE_URL = f"postgresql://{POSTGRES_USER}:{POSTGRES_PASSWORD}@{POSTGRES_HOST}:{POSTGRES_PORT}/{POSTGRES_DB}"

engine = create_engine(SQLALCHEMY_DATABASE_URL)
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)
Base = declarative_base()

def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()
