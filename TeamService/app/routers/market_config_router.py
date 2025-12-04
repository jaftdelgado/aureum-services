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


@router.get("/{public_id}", response_model=MarketConfigResponse)
def get_config(public_id: UUID, db: Session = Depends(get_db)):
    config = MarketConfigurationService.get_by_public_id(db, public_id)
    if not config:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Market config not found"
        )
    return config


@router.post("", response_model=MarketConfigResponse, status_code=status.HTTP_201_CREATED)
def create_config(data: MarketConfigCreate, db: Session = Depends(get_db)):
    return MarketConfigurationService.create(db, data)


@router.put("/{public_id}", response_model=MarketConfigResponse)
def update_config(public_id: UUID, data: MarketConfigUpdate, db: Session = Depends(get_db)):
    updated = MarketConfigurationService.update(db, public_id, data)
    if not updated:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Market config not found"
        )
    return updated
