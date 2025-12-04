from pydantic import BaseModel
from uuid import UUID
from datetime import datetime
from typing import Optional


class MarketConfigBase(BaseModel):
    teamid: UUID
    initialcash: float
    currency: str

    marketvolatility: str
    marketliquidity: str

    thickspeed: str

    transactionfee: str
    eventfrequency: str
    dividendimpact: str
    crashimpact: str

    allowshortselling: bool = False


class MarketConfigCreate(MarketConfigBase):
    pass


class MarketConfigUpdate(BaseModel):
    teamid: Optional[UUID] = None
    initialcash: Optional[float] = None
    currency: Optional[str] = None

    marketvolatility: Optional[str] = None
    marketliquidity: Optional[str] = None

    thickspeed: Optional[str] = None

    transactionfee: Optional[str] = None
    eventfrequency: Optional[str] = None
    dividendimpact: Optional[str] = None
    crashimpact: Optional[str] = None

    allowshortselling: Optional[bool] = None


class MarketConfigResponse(MarketConfigBase):
    configid: int
    publicid: UUID
    createdat: datetime
    updatedat: datetime

    class Config:
        orm_mode = True
