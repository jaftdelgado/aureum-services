from sqlalchemy import Column, Integer, ForeignKey
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.orm import relationship
from uuid import uuid4

from app.database import Base


class TeamAsset(Base):
    __tablename__ = "teamassets"

    teamassetid = Column(Integer, primary_key=True, index=True)
    publicid = Column(UUID(as_uuid=True), unique=True, default=uuid4)

    teamid = Column(Integer, ForeignKey("teams.TeamID", ondelete="CASCADE"), nullable=False)
    assetid = Column(Integer, nullable=False)

    team = relationship("Team", back_populates="team_assets")
