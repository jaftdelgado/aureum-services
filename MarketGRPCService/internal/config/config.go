package config

import (
	"os"
	"strconv"
	"time"
)

type Config struct {
	Port                string
	AssetServiceURL     string
	CourseServiceURL    string
	PortfolioServiceURL string
	TickInterval        time.Duration
	DBURL               string
}

func getenv(key, def string) string {
	if v := os.Getenv(key); v != "" {
		return v
	}
	return def
}

func GetConfig() *Config {
	port := getenv("PORT", ":50051")
	assetURL := getenv("ASSET_SERVICE_URL", "")
	courseURL := getenv("COURSE_SERVICE_URL", "")
	portfolioURL := getenv("PORTFOLIO_SERVICE_URL", "")
	intervalStr := getenv("TICK_INTERVAL_SECONDS", "4")
	intervalSeconds, err := strconv.Atoi(intervalStr)

	if err != nil || intervalSeconds <= 0 {
		intervalSeconds = 4
	}

	dbURL := getenv("MARKET_DATABASE_URL", "")

	return &Config{
		Port:                port,
		AssetServiceURL:     assetURL,
		CourseServiceURL:    courseURL,
		PortfolioServiceURL: portfolioURL,
		TickInterval:        time.Duration(intervalSeconds) * time.Second,
		DBURL:               dbURL,
	}
}
