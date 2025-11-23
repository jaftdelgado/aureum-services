from sqlalchemy import Column, Integer, String, TIMESTAMP
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.sql import func
from sqlalchemy.orm import relationship
from uuid import uuid4

from app.database import Base

class Team(Base):
    __tablename__ = "teams"

    teamid = Column(Integer, primary_key=True, index=True)
    publicid = Column(UUID(as_uuid=True), unique=True, default=uuid4)

    teamname = Column(String(48), nullable=False)
    description = Column(String(128), nullable=True)
    teampic = Column(String(255), nullable=True)

    access_code = Column(String(20), unique=True, nullable=False)

    createdat = Column(TIMESTAMP, nullable=False, server_default=func.now())
    professorid = Column(UUID(as_uuid=True), nullable=False, default=uuid4)

    market_configuration = relationship(
        "MarketConfiguration",
        uselist=False,
        back_populates="team",
        cascade="all, delete-orphan"
    )

    memberships = relationship(
        "TeamMembership",
        back_populates="team",
        cascade="all, delete"
    )

    team_assets = relationship(
        "TeamAsset",
        back_populates="team",
        cascade="all, delete"
    )