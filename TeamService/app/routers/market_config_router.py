from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from uuid import UUID

from app.database import get_db
from app.schemas.market_configuration import MarketConfigCreate, MarketConfigUpdate, MarketConfigResponse
from app.services.market_config_service import MarketConfigurationService

router = APIRouter(
    prefix="/api/market-config",
    tags=["Market Config"]
)

@router.get("/{team_id}", response_model=MarketConfigResponse,
    summary="Obtener configuracion de mercado",
    description="Recupera las reglas del simulador para un curso especifico.",
    responses={404: {"description": "Configuracion no encontrada"}}
)
def get_config(team_id: UUID, db: Session = Depends(get_db)):
    config = MarketConfigurationService.get_by_team_id(db, team_id)
    if not config:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Market config not found"
        )
    return config

@router.post("", response_model=MarketConfigResponse, status_code=status.HTTP_201_CREATED,
    summary="Crear configuracion inicial",
    description="Establece los parametros iniciales del simulador de mercado para un nuevo curso."
)
def create_config(data: MarketConfigCreate, db: Session = Depends(get_db)):
    existing = MarketConfigurationService.get_by_team_id(db, data.team_id)
    if existing:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Market config already exists for this team"
        )
    return MarketConfigurationService.create(db, data)

@router.put("/{team_id}", response_model=MarketConfigResponse,
    summary="Actualizar configuracion",
    description="Modifica los parametros del simulador de mercado."
)
def update_config(team_id: UUID, data: MarketConfigUpdate, db: Session = Depends(get_db)):
    updated = MarketConfigurationService.update(db, team_id, data)
    if not updated:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Market config not found"
        )
    return updated
