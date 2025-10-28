from sqlalchemy import Column, Integer, String, Date, ForeignKey
from sqlalchemy.orm import relationship
from app.database import Base

class Profile(Base):
    __tablename__ = "profiles"

    id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, unique=True, nullable=False)
    first_name = Column(String(32), nullable=True)
    last_name = Column(String(48), nullable=True)
    profile_pic_url = Column(String(100), nullable=True)
    profile_bio = Column(String(128), nullable=True)
    birthday = Column(Date, nullable=True)
    organization = Column(String(48), nullable=True)
