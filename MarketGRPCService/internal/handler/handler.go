package handler

import (
	"context"
	"encoding/json"
	"fmt"
	"math/rand"
	"net/http"
	"time"

	"gorm.io/gorm"

	"github.com/jaftdelgado/aureum-services/MarketGRPCService/internal/config"
	pb "github.com/jaftdelgado/aureum-services/MarketGRPCService/proto"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"
)

// Estructura que coincide con la respuesta JSON de AssetService
type Asset struct {
	AssetID     int64   `json:"assetId"`
	AssetSymbol string  `json:"assetSymbol"`
	AssetName   string  `json:"assetName"`
	AssetType   string  `json:"assetType"`
	BasePrice   float64 `json:"basePrice"`
	Volatility  float64 `json:"volatility"`
	Drift       float64 `json:"drift"`
	MaxPrice    float64 `json:"maxPrice"`
	MinPrice    float64 `json:"minPrice"`
}

// Estado interno por activo en esta conexión
type assetState struct {
	asset        Asset
	currentPrice float64
}

type MarketHandler struct {
	pb.UnimplementedMarketServiceServer
	cfg        *config.Config
	httpClient *http.Client
	db         *gorm.DB
}

func NewMarketHandler(cfg *config.Config, db *gorm.DB) *MarketHandler {
	return &MarketHandler{
		cfg: cfg,
		db:  db,
		httpClient: &http.Client{
			Timeout: 5 * time.Second,
		},
	}
}

// Llama al microservicio AssetService y obtiene la lista de activos
func (h *MarketHandler) fetchAssets(ctx context.Context) ([]Asset, error) {
	req, err := http.NewRequestWithContext(ctx, http.MethodGet, h.cfg.AssetServiceURL, nil)
	if err != nil {
		return nil, fmt.Errorf("crear request a AssetService: %w", err)
	}

	resp, err := h.httpClient.Do(req)
	if err != nil {
		return nil, fmt.Errorf("llamar a AssetService: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("AssetService devolvió status %d", resp.StatusCode)
	}

	var assets []Asset
	if err := json.NewDecoder(resp.Body).Decode(&assets); err != nil {
		return nil, fmt.Errorf("decodificar respuesta AssetService: %w", err)
	}

	return assets, nil
}

// Calcula el siguiente precio usando la volatilidad
// Regla simple: nuevoPrecio = precioActual * (1 + cambio),
// donde cambio ∈ [-volatility, +volatility]
func nextPrice(prev float64, a Asset) float64 {
	vol := a.Volatility
	if vol <= 0 {
		vol = 0.01 // por si viene 0, que se mueva un poco
	}

	// random entre -1 y 1
	r := (rand.Float64() * 2) - 1
	change := r * vol

	newPrice := prev * (1 + change)

	// Respetar límites si existen
	if a.MaxPrice > 0 && newPrice > a.MaxPrice {
		newPrice = a.MaxPrice
	}
	if a.MinPrice > 0 && newPrice < a.MinPrice {
		newPrice = a.MinPrice
	}
	if newPrice <= 0 {
		newPrice = prev // nunca dejarlo en cero o negativo
	}

	return newPrice
}

// Implementación del RPC CheckMarket
func (h *MarketHandler) CheckMarket(req *pb.MarketRequest, stream pb.MarketService_CheckMarketServer) error {
	ctx := stream.Context()

	// Leemos los activos una vez al inicio
	assets, err := h.fetchAssets(ctx)
	if err != nil {
		return status.Errorf(codes.Internal, "no se pudo obtener activos: %v", err)
	}

	if len(assets) == 0 {
		return status.Error(codes.NotFound, "no hay activos en el sistema")
	}

	// Seed del random (solo una vez)
	rand.Seed(time.Now().UnixNano())

	// Estado interno de precios (iniciamos en basePrice)
	states := make([]*assetState, 0, len(assets))
	for _, a := range assets {
		price := a.BasePrice
		if price <= 0 {
			price = 1 // por si acaso
		}
		states = append(states, &assetState{
			asset:        a,
			currentPrice: price,
		})
	}

	// Intervalo de actualización
	interval := h.cfg.TickInterval
	if req.GetIntervalSeconds() > 0 {
		interval = time.Duration(req.GetIntervalSeconds()) * time.Second
	}

	ticker := time.NewTicker(interval)
	defer ticker.Stop()

	for {
		select {
		case <-ctx.Done():
			// El cliente cerró la conexión
			return ctx.Err()

		case t := <-ticker.C:
			// Actualizamos todos los precios
			for _, s := range states {
				s.currentPrice = nextPrice(s.currentPrice, s.asset)
			}

			// Construimos respuesta
			resp := &pb.MarketResponse{
				TimestampUnixMillis: t.UnixMilli(),
				Assets:              make([]*pb.MarketAsset, 0, len(states)),
			}

			for _, s := range states {
				resp.Assets = append(resp.Assets, &pb.MarketAsset{
					Id:         int32(s.asset.AssetID),
					Symbol:     s.asset.AssetSymbol,
					Name:       s.asset.AssetName,
					Price:      s.currentPrice,
					BasePrice:  s.asset.BasePrice,
					Volatility: s.asset.Volatility,
				})
			}

			if err := stream.Send(resp); err != nil {
				return err
			}
		}
	}
}
