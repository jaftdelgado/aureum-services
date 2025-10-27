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

# Perfil base
class ProfileBase(BaseModel):
    first_name: str | None = None
    last_name: str | None = None
    profile_pic_url: str | None = None
    profile_bio: str | None = None
    birthday: str | None = None
    organization: str | None = None

# Perfil completo
class Profile(ProfileBase):
    id: int
    user_id: int

    class Config:
        from_attributes = True

# Extender el esquema de creación de usuario
class UserRegister(UserCreate):
    profile: ProfileBase | None = None