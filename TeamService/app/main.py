from fastapi import FastAPI
from app.database import Base, engine
from app.routers.market_config_router import router as market_config_router

Base.metadata.create_all(bind=engine)

app = FastAPI(
    title="TeamService",
    version="1.0.0",
)

# Registrar router único
app.include_router(market_config_router)

@app.get("/")
def root():
    return {"message": "TeamService running"}
