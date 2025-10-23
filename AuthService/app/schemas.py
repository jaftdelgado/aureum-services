from enum import Enum
from pydantic import BaseModel, EmailStr, Field

# Base schema for shared attributes
class UserBase(BaseModel):
    email_address: EmailStr
    username: str
    is_active: bool = True
    role_id: int = 2

class UserLogin(BaseModel):
    identifier: str
    password: str = Field(format="password")

# Schema for creating a user (registration). Includes password.
class UserCreate(UserBase):
    password: str = Field(format="password")

# Schema for what we return to the client.
class User(UserBase):
    id: int

    class Config:
        from_attributes = True  # Allows ORM mode (translates ORM object -> Pydantic model)

# Schema for the login request
class Token(BaseModel):
    access_token: str
    token_type: str

# Schema for the data embedded inside the JWT token
class TokenData(BaseModel):
    user_id: int | None = None