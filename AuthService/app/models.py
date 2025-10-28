# app/models.py
from sqlalchemy import Boolean, Column, ForeignKey, Integer, String, DateTime, func
from sqlalchemy.orm import relationship
from app.database import Base

class Role(Base):
    __tablename__ = "roles"  # <-- doble underscore

    id = Column(Integer, primary_key=True, index=True)
    name = Column(String(20), nullable=False)

    users = relationship("User", back_populates="role")

class User(Base):
    __tablename__ = "users"  # <-- doble underscore

    id = Column(Integer, primary_key=True, index=True)
    email_address = Column(String(50), unique=True, index=True, nullable=False)
    hashed_password = Column(String, nullable=False)
    username = Column(String(50), unique=True, nullable=False)
    is_active = Column(Boolean, default=True)
    role_id = Column(Integer, ForeignKey("roles.id"), nullable=False)
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), onupdate=func.now())

    role = relationship("Role", back_populates="users")