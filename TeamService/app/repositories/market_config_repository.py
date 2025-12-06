from sqlalchemy.orm import Session
from uuid import UUID
from app.models.market_configuration import MarketConfiguration
from app.schemas.market_configuration import MarketConfigCreate, MarketConfigUpdate

class MarketConfigRepository:

    @staticmethod
    def get_by_team_id(db: Session, team_id: UUID) -> MarketConfiguration | None:
        return db.query(MarketConfiguration).filter(
            MarketConfiguration.team_id == team_id
        ).first()

    @staticmethod
    def create(db: Session, data: MarketConfigCreate) -> MarketConfiguration:
        new_config = MarketConfiguration(**data.dict())
        db.add(new_config)
        db.commit()
        db.refresh(new_config)
        return new_config

    @staticmethod
    def update(db: Session, team_id: UUID, data: MarketConfigUpdate) -> MarketConfiguration | None:
        config = MarketConfigRepository.get_by_team_id(db, team_id)
        if not config:
            return None
        update_data = data.dict(exclude_unset=True)
        for key, value in update_data.items():
            setattr(config, key, value)
        db.commit()
        db.refresh(config)
        return config
