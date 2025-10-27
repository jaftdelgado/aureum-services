from sqlalchemy.orm import Session
from app import models, schemas

def create_profile(db: Session, user_id: int, profile_data: schemas.ProfileBase):
    db_profile = models.Profile(
        user_id=user_id,
        first_name=profile_data.first_name,
        last_name=profile_data.last_name,
        profile_pic_url=profile_data.profile_pic_url,
        profile_bio=profile_data.profile_bio,
        birthday=profile_data.birthday,
        organization=profile_data.organization
    )
    db.add(db_profile)
    db.commit()
    db.refresh(db_profile)
    return db_profile