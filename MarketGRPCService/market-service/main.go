package main

import (
	"log"

	"github.com/joho/godotenv"

	"github.com/jaftdelgado/aureum-services/MarketGRPCService/internal/config"
	dbpkg "github.com/jaftdelgado/aureum-services/MarketGRPCService/internal/db"
	"github.com/jaftdelgado/aureum-services/MarketGRPCService/internal/handler"
	"github.com/jaftdelgado/aureum-services/MarketGRPCService/internal/server"
)

func main() {
	godotenv.Load()
	log.Println("Cargando configuración...")
	cfg := config.GetConfig()

	log.Println("Conectando a la base...")
	db, err := dbpkg.NewDB(cfg)
	if err != nil {
		log.Fatalf(" Error al conectar a la base de datos: %v", err)
	}

	log.Println("DB conectada correctamente")

	h := handler.NewMarketHandler(cfg, db)

	log.Println("Levantando servidor gRPC...")
	if err := server.Run(cfg.Port, h); err != nil {
		log.Fatalf(" Error al iniciar MarketGRPCService: %v", err)
	}
}
