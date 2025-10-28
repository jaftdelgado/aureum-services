from fastapi import FastAPI, Depends
from app.database import get_db
from app.repositories import profile_repository
from app import schemas

app = FastAPI(title="UserProfileService")

@app.post("/profiles", response_model=schemas.Profile)
def create_profile(profile: schemas.ProfileCreate, db=Depends(get_db)):
    return profile_repository.create_profile(db, profile)

@app.get("/health")
def health_check():
    return {"status": "ok"}
