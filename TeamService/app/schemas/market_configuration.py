from pydantic import BaseModel
from uuid import UUID
from datetime import datetime
from typing import Optional

class MarketConfigBase(BaseModel):
    team_id: UUID
    initial_cash: float
    currency: str

    market_volatility: str
    market_liquidity: str
    thick_speed: str

    transaction_fee: str
    event_frequency: str
    dividend_impact: str
    crash_impact: str

    allow_short_selling: bool = False


class MarketConfigCreate(MarketConfigBase):
    pass


class MarketConfigUpdate(BaseModel):
    team_id: Optional[UUID] = None
    initial_cash: Optional[float] = None
    currency: Optional[str] = None

    market_volatility: Optional[str] = None
    market_liquidity: Optional[str] = None
    thick_speed: Optional[str] = None

    transaction_fee: Optional[str] = None
    event_frequency: Optional[str] = None
    dividend_impact: Optional[str] = None
    crash_impact: Optional[str] = None

    allow_short_selling: Optional[bool] = None


class MarketConfigResponse(MarketConfigBase):
    public_id: UUID
    
    model_config = {
        "from_attributes": True
    }
