from sqlalchemy import Column, Integer, String, DateTime
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.sql import func
from sqlalchemy.orm import relationship
import uuid 

from app.database import Base

class Team(Base):
    __tablename__ = "teams"

    team_id = Column("teamid", Integer, primary_key=True, index=True, autoincrement=True)
    
    public_id = Column("publicid", UUID(as_uuid=True), default=uuid.uuid4, unique=True, nullable=False)
    
    professor_id = Column("professorid", UUID(as_uuid=True), nullable=False)
    name = Column("teamname", String(48), nullable=False)
    description = Column("description", String(128), nullable=True)
    team_pic = Column("teampic", String(255), nullable=True)
    
    access_code = Column("accesscode", String(20), unique=True, nullable=False)
    
    created_at = Column("createdat", DateTime(timezone=True), server_default=func.now())

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
