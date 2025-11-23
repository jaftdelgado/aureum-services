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

def update_profile(db: Session, auth_id: str, update_data: schemas.ProfileUpdateDTO):
    db_profile = get_profile_by_auth_id(db, auth_id)
    if not db_profile:
        return None
    
    update_dict = update_data.model_dump(exclude_unset=True)
    
    for key, value in update_dict.items():
        setattr(db_profile, key, value)
        
    db.add(db_profile)
    db.commit()
    db.refresh(db_profile)
    return db_profile

def update_profile_pic(db: Session, auth_id: str, mongo_id: str):
    db_profile = get_profile_by_auth_id(db, auth_id)
    if db_profile:
        db_profile.profile_pic_id = mongo_id
        db.add(db_profile)
        db.commit()
        db.refresh(db_profile)
    return db_profile

def delete_profile(db: Session, auth_id: str):
    db_profile = get_profile_by_auth_id(db, auth_id)
    if db_profile:
        db.delete(db_profile)
        db.commit()
        return True
    return False