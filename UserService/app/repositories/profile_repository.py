from sqlalchemy.orm import Session
from app import models, schemas

def create_profile(db: Session, profile: schemas.ProfileCreate):
    db_profile = models.Profile(
        user_id=profile.user_id,
        first_name=profile.first_name,
        last_name=profile.last_name,
        profile_pic_url=profile.profile_pic_url,
        profile_bio=profile.profile_bio,
        birthday=profile.birthday,
        organization=profile.organization
    )
    
    db.add(db_profile)
    db.commit()
    db.refresh(db_profile)
    
    return db_profile
