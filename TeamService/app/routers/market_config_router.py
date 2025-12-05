from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from uuid import UUID

from app.database import get_db

from app.schemas.market_configuration import (
    MarketConfigCreate,
    MarketConfigUpdate,
    MarketConfigResponse,
)

from app.services.market_config_service import MarketConfigurationService


router = APIRouter(
    prefix="/api/market-config",
    tags=["Market Config"]
)


@router.get("/{team_id}", response_model=MarketConfigResponse,
    summary="Obtener configuración de mercado",
    description="Recupera las reglas del simulador (saldo inicial, comisiones, etc.) para un curso específico.",
    responses={404: {"description": "Configuración no encontrada"}}
)
def get_config(publicid: UUID, db: Session = Depends(get_db)):
    config = MarketConfigurationService.get_by_public_id(db, publicid)
    if not config:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Market config not found"
        )
    return config


@router.post("", response_model=MarketConfigResponse, status_code=status.HTTP_201_CREATED,
    summary="Crear configuración inicial",
    description="Establece los parámetros iniciales del simulador de mercado para un nuevo curso."
)
def create_config(data: MarketConfigCreate, db: Session = Depends(get_db)):
    return MarketConfigurationService.create(db, data)


@router.put("/{config_id}", response_model=MarketConfigResponse,
    summary="Actualizar configuración",
    description="Modifica los parámetros del simulador de mercado."
)
def update_config(publicid: UUID, data: MarketConfigUpdate, db: Session = Depends(get_db)):
    updated = MarketConfigurationService.update(db, publicid, data)
    if not updated:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Market config not found"
        )
    return updated
