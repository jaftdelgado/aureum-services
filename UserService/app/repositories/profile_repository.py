from sqlalchemy.orm import Session
from ..models import Profile
from ..schemas import ProfileCreateDTO

def get_profile_by_auth_id(db: Session, auth_id: str):
    return db.query(models.Profile).filter(models.Profile.auth_user_id == auth_id).first()

def create_profile(db: Session, profile_data: ProfileCreateDTO):
    db_profile = Profile(**profile_data.model_dump())
    
    db.add(db_profile)
    db.commit()
    db.refresh(db_profile) 
    return db_profile

def get_profile_by_username(db: Session, username: str):
    return db.query(Profile).filter(Profile.username == username).first()

def get_profile_by_auth_id(db: Session, auth_id: str):
    return db.query(Profile).filter(Profile.auth_user_id == auth_id).first()
