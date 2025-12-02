from sqlalchemy import (
    Column,
    Integer,
    String,
    Boolean,
    TIMESTAMP,
    ForeignKey,
    Double,
)
from sqlalchemy.dialects.postgresql import UUID, ENUM
from sqlalchemy.sql import func
from sqlalchemy.orm import relationship
from uuid import uuid4

from app.database import Base

currency_enum = ENUM(
    "USD", "EUR", "MXN",
    name="currency_enum",
    create_type=True
)

volatility_enum = ENUM(
    "High", "Medium", "Low", "Disabled",
    name="volatility_enum",
    create_type=True
)

thick_speed_enum = ENUM(
    "High", "Medium", "Low",
    name="thick_speed_enum",
    create_type=True
)

transaction_fee_enum = ENUM(
    "High", "Medium", "Low", "Disabled",
    name="transaction_fee_enum",
    create_type=True
)

class MarketConfiguration(Base):
    __tablename__ = "marketconfigurations"

    configid = Column(Integer, primary_key=True, index=True)
    publicid = Column(UUID(as_uuid=True), unique=True, default=uuid4)

    teamid = Column(Integer, ForeignKey("teams.teamid", ondelete="CASCADE"), unique=True, nullable=False)

    initialcash = Column(Double, nullable=False)
    currency = Column(currency_enum, nullable=False)

    marketvolatility = Column(volatility_enum, nullable=False)
    marketliquidity = Column(volatility_enum, nullable=False)

    thickspeed = Column(thick_speed_enum, nullable=False)

    transactionfee = Column(transaction_fee_enum, nullable=False)
    eventfrequency = Column(transaction_fee_enum, nullable=False)
    dividendimpact = Column(transaction_fee_enum, nullable=False)
    crashimpact = Column(transaction_fee_enum, nullable=False)

    allowshortselling = Column(Boolean, nullable=False, default=False)

    createdat = Column(TIMESTAMP, server_default=func.now())
    updatedat = Column(TIMESTAMP, server_default=func.now(), onupdate=func.now())

    team = relationship("Team", back_populates="market_configuration")
