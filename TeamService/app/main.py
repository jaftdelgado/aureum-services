from fastapi import FastAPI
from app.database import Base, engine
from app.routers.market_config_router import router as market_config_router
from app.routers.team_member_router import router as team_member_router
from app.routers.team_router import router as team_router

Base.metadata.create_all(bind=engine)

app = FastAPI(
    title="TeamService",
    version="1.0.0",
)

app.include_router(market_config_router)
app.include_router(team_member_router)
app.include_router(team_router)

@app.get("/")
def root():
    return {"message": "TeamService running"}
