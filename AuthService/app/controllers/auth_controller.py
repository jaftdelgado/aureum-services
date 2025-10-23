from datetime import timedelta
from fastapi import Depends
from fastapi.security import OAuth2PasswordBearer
from sqlalchemy.orm import Session

from app import schemas, security
from app.database import get_db
from app.repositories import user_repository

oauth2_scheme = OAuth2PasswordBearer(tokenUrl="login")

class AuthController:
    def __init__(self, db: Session):
        self.db = db

    def register_user(self, user_data: schemas.UserCreate) -> schemas.User:
        if user_repository.get_user_by_email(self.db, user_data.email):
            raise ValueError("Email already registered")
        
        if user_repository.get_user_by_username(self.db, user_data.username):
            raise ValueError("Username already taken")
        
        if len(user_data.password) < 8:
            raise ValueError("Password must be at least 8 characters long")
        
        return user_repository.create_user(self.db, user_data)
    
    def login_user(self, identifier: str, password: str) -> schemas.Token:
        user = user_repository.get_user_by_email(self.db, identifier)
        if not user:
            user = user_repository.get_user_by_username(self.db, identifier)

        if not user or not security.verify_password(password, user.hashed_password):
            raise ValueError("Incorrect credentials")
        access_token_expires = timedelta(minutes=security.ACCESS_TOKEN_EXPIRE_MINUTES)
        access_token = security.create_access_token(
            data={"sub": str(user.id)},
            expires_delta=access_token_expires
        )

        return schemas.Token(access_token=access_token, token_type="bearer")
    
    def get_user_by_id(self, user_id: int) -> schemas.User:
        user = user_repository.get_user_by_id(self.db, user_id)
        if not user:
            raise ValueError("User not found")
        return user
    
def get_auth_controller(db: Session = Depends(get_db)):
    return AuthController(db)