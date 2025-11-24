package config

import (
	"os"
	"strconv"
	"time"
)

type Config struct {
	Port            string
	AssetServiceURL string
	TickInterval    time.Duration
}

func getenv(key, def string) string {
	if v := os.Getenv(key); v != "" {
		return v
	}
	return def
}

func GetConfig() *Config {
	// Railway siempre expone el puerto en la variable PORT.
	// Si existe PORT, la usamos; si no, caemos a MARKET_GRPC_PORT o 50051.
	port := os.Getenv("PORT")
	if port == "" {
		port = getenv("MARKET_GRPC_PORT", "50051")
	}

	// URL del AssetService (sobrescrita por variable de entorno en Railway)
	assetURL := getenv("ASSET_SERVICE_URL", "http://assetservice.railway.internal:5000/assets")

	intervalStr := getenv("TICK_INTERVAL_SECONDS", "4")
	intervalSeconds, err := strconv.Atoi(intervalStr)
	if err != nil || intervalSeconds <= 0 {
		intervalSeconds = 4
	}

	return &Config{
		Port:            port,
		AssetServiceURL: assetURL,
		TickInterval:    time.Duration(intervalSeconds) * time.Second,
	}
}
