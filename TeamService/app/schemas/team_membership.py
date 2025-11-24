from pydantic import BaseModel
from uuid import UUID
from datetime import datetime
from pydantic import BaseModel, Field
from typing import Optional


class TeamMembershipBase(BaseModel):
    teamid: int
    userid: int


class JoinCourseDTO(BaseModel):
    access_code: str = Field(..., min_length=3)
    user_id: int 


class TeamMembershipUpdate(BaseModel):
    teamid: Optional[int] = None
    userid: Optional[int] = None


class TeamMembershipResponse(BaseModel):
    membershipid: int
    publicid: UUID
    teamid: int
    userid: int
    joinedat: datetime

    class Config:
        from_attributes = True