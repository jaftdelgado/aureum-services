import os
from dotenv import load_dotenv
from sqlalchemy import create_engine
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker

load_dotenv()

DATABASE_URL = os.getenv("DATABASE_URL")

if not DATABASE_URL:
    user = os.getenv("USUARIOS_DB_USER", "postgres")
    password = os.getenv("USUARIOS_DB_PASSWORD", "password")
    host = os.getenv("USUARIOS_DB_HOST", "localhost")
    port = os.getenv("USUARIOS_DB_PORT", "5432")
    db_name = os.getenv("USUARIOS_DB_NAME", "users_db")
    DATABASE_URL = f"postgresql://{user}:{password}@{host}:{port}/{db_name}"

if DATABASE_URL and DATABASE_URL.startswith("postgres://"):
    DATABASE_URL = DATABASE_URL.replace("postgres://", "postgresql://", 1)

engine = create_engine(DATABASE_URL, pool_pre_ping=True)
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)
Base = declarative_base()

def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()
