from sqlalchemy.orm import Session
from app import models, schemas
from app.security import get_password_hash

def get_user_by_email(db: Session, email_address: str):
    """Find a user by their email address."""
    return db.query(models.User).filter(models.User.email_address == email_address).first()

def get_user_by_username(db: Session, username: str):
    """Find a user by their username"""
    return db.query(models.User).filter(models.User.username == username).first()


def get_user_by_id(db: Session, user_id: int):
    """Find a user by their ID."""
    return db.query(models.User).filter(models.User.id == user_id).first()

def create_user(db: Session, user: schemas.UserCreate):
    if user.role_id is None:
        user.role_id = 2 
    hashed_password = get_password_hash(user.password)
    db_user = models.User(
        email_address=user.email_address,
        hashed_password=hashed_password,
        username=user.username,
        role_id=user.role_id
    )
    db.add(db_user)
    db.commit()
    db.refresh(db_user) # Refresh to get the ID from the database
    return db_user

def update_user_password(db: Session, user_id: int, new_password: str):
    user = db.query(models.User).filter(models.User.id == user_id).first()
    if user: 
        user.hashed_password = get_password_hash(new_password)
        db.commit()
        db.refresh(user)
    return user