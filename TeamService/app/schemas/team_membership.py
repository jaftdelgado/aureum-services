from pydantic import BaseModel
from uuid import UUID
from datetime import datetime
from pydantic import BaseModel, Field
from typing import Optional


class TeamMembershipBase(BaseModel):
    teamid: UUID
    userid: UUID


class JoinCourseDTO(BaseModel):
    access_code: str = Field(..., min_length=3)
    user_id: UUID 


class TeamMembershipUpdate(BaseModel):
    teamid: Optional[UUID] = None
    userid: Optional[UUID] = None


class TeamMembershipResponse(BaseModel):
    membershipid: int
    publicid: UUID
    teamid: UUID
    userid: UUID
    joinedat: datetime

    class Config:
        from_attributes = True