from pydantic import BaseModel, UUID4, Field
from typing import Optional
from datetime import datetime

class TeamCreateDTO(BaseModel):
    name: str = Field(..., min_length=3, max_length=48)
    description: Optional[str] = Field(None, max_length=128)
    professor_id: int 

class TeamResponseDTO(BaseModel):
    public_id: UUID4
    name: str
    description: Optional[str]
    professor_id: int
    access_code: str 
    team_pic: Optional[str]
    created_at: datetime

    class Config:
        from_attributes = True

class TeamUpdateDTO(BaseModel):
    name: Optional[str] = None
    description: Optional[str] = None