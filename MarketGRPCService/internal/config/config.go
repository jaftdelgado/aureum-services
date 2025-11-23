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
	port := getenv("MARKET_GRPC_PORT", "50051")
	assetURL := getenv("ASSET_SERVICE_URL", "https://assetservice-production.up.railway.app/assets")

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
