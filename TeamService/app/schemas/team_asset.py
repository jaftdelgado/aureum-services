from pydantic import BaseModel
from uuid import UUID
from typing import Optional


class TeamAssetBase(BaseModel):
    teamid: int
    assetid: int


class TeamAssetCreate(TeamAssetBase):
    pass


class TeamAssetUpdate(BaseModel):
    teamid: Optional[int] = None
    assetid: Optional[int] = None


class TeamAssetResponse(TeamAssetBase):
    teamassetid: int
    publicid: UUID

    class Config:
        orm_mode = True
