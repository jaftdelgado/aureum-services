from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from .controllers import profile_controller
from .database import engine, Base

Base.metadata.create_all(bind=engine)

app = FastAPI(title="UserService")

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(profile_controller.router, prefix="/api/v1/profiles")

@app.get("/health")
def health_check():
    return {"status": "ok", "service": "UserService"}