from sqlalchemy.orm import Session
from uuid import UUID

from app.models.market_configuration import MarketConfiguration

from app.schemas.market_configuration import (
    MarketConfigCreate,
    MarketConfigUpdate,
)


class MarketConfigurationService:

    @staticmethod
    def get_by_public_id(db: Session, public_id: UUID) -> MarketConfiguration | None:
        return (
            db.query(MarketConfiguration)
            .filter(MarketConfiguration.publicid == public_id)
            .first()
        )

    @staticmethod
    def create(db: Session, data: MarketConfigCreate) -> MarketConfiguration:
        existing_config = db.query(MarketConfiguration).filter(
            MarketConfiguration.public_id == config_dto.public_id
        ).first()
    
        if existing_config:
            raise Exception("Configuration already exists for this team")

        new_config = MarketConfiguration(**data.dict())
        db.add(new_config)
        db.commit()
        db.refresh(new_config)
        return new_config

    @staticmethod
    def update(
        db: Session,
        public_id: UUID,
        data: MarketConfigUpdate
    ) -> MarketConfiguration | None:

        config = (
            db.query(MarketConfiguration)
            .filter(MarketConfiguration.publicid == public_id)
            .first()
        )

        if not config:
            return None

        update_data = data.dict(exclude_unset=True)

        for key, value in update_data.items():
            setattr(config, key, value)

        db.commit()
        db.refresh(config)

        return config
