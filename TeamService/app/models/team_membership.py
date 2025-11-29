from sqlalchemy import Column, Integer, TIMESTAMP, ForeignKey
from sqlalchemy.dialects.postgresql import UUID
from sqlalchemy.sql import func
from sqlalchemy.orm import relationship
import uuid

from app.database import Base


class TeamMembership(Base):
    __tablename__ = "teammemberships"

    membershipid = Column(Integer, primary_key=True, index=True)
    publicid = Column(UUID(as_uuid=True), unique=True, default=uuid.uuid4)

    teamid = Column(UUID(as_uuid=True), ForeignKey("teams.publicid", ondelete="CASCADE"), nullable=False)
    userid = Column(UUID(as_uuid=True), nullable=False)

    joinedat = Column(TIMESTAMP, server_default=func.now())

    team = relationship("Team", back_populates="memberships")
