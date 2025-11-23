from pydantic import BaseModel, UUID4, Field
from typing import Optional
from enum import Enum
from datetime import datetime

class UserRoleEnum(str, Enum):
    student = "student"
    professor = "professor"

class ProfileCreateDTO(BaseModel):
    auth_user_id: UUID4 = Field(..., description="El UUID que retorna Supabase al registrarse")
    username: str = Field(..., min_length=3, max_length=50)
    full_name: str = Field(..., min_length=1, max_length=100)
    role: UserRoleEnum

class ProfileResponseDTO(ProfileCreateDTO):
    profile_id: int
    bio: Optional[str] = None
    profile_pic_id: Optional[str] = None
    created_at: datetime
    updated_at: datetime

    class Config:
        from_attributes = True

class ProfileUpdateDTO(BaseModel):
    full_name: Optional[str] = Field(None, min_length=1, max_length=100)
    bio: Optional[str] = None