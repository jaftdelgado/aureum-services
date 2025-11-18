from pydantic import BaseModel
from uuid import UUID
from datetime import datetime
from typing import Optional


class TeamMembershipBase(BaseModel):
    teamid: int
    userid: int


class TeamMembershipCreate(TeamMembershipBase):
    pass


class TeamMembershipUpdate(BaseModel):
    teamid: Optional[int] = None
    userid: Optional[int] = None


class TeamMembershipResponse(TeamMembershipBase):
    membershipid: int
    publicid: UUID
    joinedat: datetime

    class Config:
        orm_mode = True
