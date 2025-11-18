from pydantic import BaseModel
from uuid import UUID
from datetime import datetime
from typing import Optional


class TeamBase(BaseModel):
    teamname: str
    description: Optional[str] = None
    teampic: Optional[str] = None
    professorid: UUID


class TeamCreate(TeamBase):
    pass


class TeamUpdate(BaseModel):
    teamname: Optional[str] = None
    description: Optional[str] = None
    teampic: Optional[str] = None


class TeamResponse(TeamBase):
    teamid: int
    publicid: UUID
    createdat: datetime

    class Config:
        orm_mode = True
