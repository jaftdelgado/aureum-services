package main

import (
	"log"

	"github.com/jaftdelgado/aureum-services/MarketGRPCService/internal/config"
	"github.com/jaftdelgado/aureum-services/MarketGRPCService/internal/handler"
	"github.com/jaftdelgado/aureum-services/MarketGRPCService/internal/server"
)

func main() {
	cfg := config.GetConfig()

	h := handler.NewMarketHandler(cfg)

	if err := server.Run("0.0.0.0:" + cfg.Port, h); err != nil { ... }
		log.Fatalf("Error al iniciar MarketGRPCService: %v", err)
	}
}
