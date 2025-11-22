from sqlalchemy import Column, Integer, String, Text, DateTime, Enum as SqEnum
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.sql import func
import enum
from .database import Base

class UserRole(str, enum.Enum):
    student = "student"
    professor = "professor"

class Profile(Base):
    __tablename__ = "profiles"

    profile_id = Column(Integer, primary_key=True, index=True)
    
    auth_user_id = Column(UUID(as_uuid=True), unique=True, nullable=False, index=True)
    
    username = Column(String(50), unique=True, nullable=False)
    full_name = Column(String(100), nullable=False)
    
    bio = Column(Text, nullable=True)
    profile_pic_id = Column(String(50), nullable=True)

    role = Column(SqEnum(UserRole), default=UserRole.student, nullable=False)

    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), server_default=func.now(), onupdate=func.now())