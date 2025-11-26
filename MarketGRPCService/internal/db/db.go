package db

import (
	"fmt"
	"time"

	"github.com/jaftdelgado/aureum-services/MarketGRPCService/internal/config"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"
)

func NewDB(cfg *config.Config) (*gorm.DB, error) {
	if cfg.DBURL == "" {
		return nil, fmt.Errorf("DBURL vacío, define MARKET_DATABASE_URL en las variables de entorno")
	}

	dsn := cfg.DBURL

	db, err := gorm.Open(postgres.Open(dsn), &gorm.Config{})
	if err != nil {
		return nil, fmt.Errorf("error al abrir conexión con la base: %w", err)
	}

	sqlDB, err := db.DB()
	if err != nil {
		return nil, fmt.Errorf("error al obtener sql.DB desde gorm: %w", err)
	}

	sqlDB.SetMaxIdleConns(10)
	sqlDB.SetMaxOpenConns(25)
	sqlDB.SetConnMaxLifetime(1 * time.Hour)

	return db, nil
}
