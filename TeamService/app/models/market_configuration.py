from sqlalchemy import Column, Integer, Boolean, TIMESTAMP, Float, String
from sqlalchemy.dialects.postgresql import UUID, ENUM
from sqlalchemy.sql import func
from uuid import uuid4

from app.database import Base

currency_enum = ENUM("USD", "EUR", "MXN", name="currency_enum", create_type=True)
volatility_enum = ENUM("High", "Medium", "Low", "Disabled", name="volatility_enum", create_type=True)
thick_speed_enum = ENUM("High", "Medium", "Low", name="thick_speed_enum", create_type=True)
transaction_fee_enum = ENUM("High", "Medium", "Low", "Disabled", name="transaction_fee_enum", create_type=True)

class MarketConfiguration(Base):
    __tablename__ = "marketconfigurations"

    config_id = Column("configid", Integer, primary_key=True, index=True, autoincrement=True)
    public_id = Column("publicid", UUID(as_uuid=True), default=uuid4, unique=True, nullable=False)
    team_id = Column("teamid", UUID(as_uuid=True), unique=True, nullable=False)

    initial_cash = Column("initialcash", Float, nullable=False)
    currency = Column("currency", currency_enum, nullable=False)

    market_volatility = Column("marketvolatility", volatility_enum, nullable=False)
    market_liquidity = Column("marketliquidity", volatility_enum, nullable=False)
    thick_speed = Column("thickspeed", thick_speed_enum, nullable=False)

    transaction_fee = Column("transactionfee", transaction_fee_enum, nullable=False)
    event_frequency = Column("eventfrequency", transaction_fee_enum, nullable=False)
    dividend_impact = Column("dividendimpact", transaction_fee_enum, nullable=False)
    crash_impact = Column("crashimpact", transaction_fee_enum, nullable=False)

    allow_short_selling = Column("allowshortselling", Boolean, nullable=False, default=False)

    created_at = Column("createdat", TIMESTAMP(timezone=True), server_default=func.now())
    updated_at = Column("updatedat", TIMESTAMP(timezone=True), server_default=func.now(), onupdate=func.now())
