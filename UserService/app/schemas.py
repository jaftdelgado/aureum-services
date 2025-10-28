from pydantic import BaseModel

class ProfileBase(BaseModel):
    first_name: str | None = None
    last_name: str | None = None
    profile_pic_url: str | None = None
    profile_bio: str | None = None
    birthday: str | None = None
    organization: str | None = None

class ProfileCreate(ProfileBase):
    user_id: int

class Profile(ProfileBase):
    id: int
    user_id: int

    class Config:
        from_attributes = True
