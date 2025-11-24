from sqlalchemy import Column, Integer, String, DateTime
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.sql import func
from sqlalchemy.orm import relationship
import uuid

from app.database import Base

class Team(Base):
    __tablename__ = "teams"

    team_id = Column("TeamID", Integer, primary_key=True, index=True, autoincrement=True)
    
    public_id = Column("PublicID", UUID(as_uuid=True), default=uuid.uuid4, unique=True, nullable=False)
    
    professor_id = Column("ProfessorID", Integer, nullable=False)
    name = Column("TeamName", String(48), nullable=False)
    description = Column("Description", String(128), nullable=True)
    team_pic = Column("TeamPic", String(255), nullable=True)
    
    access_code = Column("AccessCode", String(20), unique=True, nullable=False)
    
    created_at = Column("CreatedAt", DateTime(timezone=True), server_default=func.now())

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
