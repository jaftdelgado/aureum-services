from sqlalchemy.orm import Session
from uuid import UUID
from app.schemas.market_configuration import MarketConfigCreate, MarketConfigUpdate
from app.repositories.market_config_repository import MarketConfigRepository

class MarketConfigurationService:

    @staticmethod
    def get_by_team_id(db: Session, team_id: UUID):
        return MarketConfigRepository.get_by_team_id(db, team_id)

    @staticmethod
    def create(db: Session, data: MarketConfigCreate):
        return MarketConfigRepository.create(db, data)

    @staticmethod
    def update(db: Session, team_id: UUID, data: MarketConfigUpdate):
        return MarketConfigRepository.update(db, team_id, data)
