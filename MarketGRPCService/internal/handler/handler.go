package handler

import (
	"context"
	"encoding/json"
	"fmt"
	"log"
	"math/rand"
	"net/http"
	"time"

	"gorm.io/gorm"

	"github.com/jaftdelgado/aureum-services/MarketGRPCService/internal/config"
	"github.com/jaftdelgado/aureum-services/MarketGRPCService/internal/db"
	pb "github.com/jaftdelgado/aureum-services/MarketGRPCService/proto"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"
)

type TeamAsset struct {
	TeamAssetID  int64   `json:"teamAssetId"`
	PublicID     string  `json:"publicId"`
	TeamID       string  `json:"teamId"`
	AssetID      string  `json:"assetId"`      // uuid del asset-team
	CurrentPrice float64 `json:"currentPrice"` // precio actual del team
	Asset        Asset   `json:"asset"`        // datos del asset anidado
}

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

type assetState struct {
	asset        Asset
	teamAssetID  int
	currentPrice float64
}

type MarketHandler struct {
	pb.UnimplementedMarketServiceServer
	cfg        *config.Config
	httpClient *http.Client
	db         *gorm.DB
	logger     *log.Logger
}

type RemoteTeamAsset struct {
	TeamAssetID  int     `json:"teamAssetId"`
	PublicID     string  `json:"publicId"`
	TeamID       string  `json:"teamId"`
	AssetID      string  `json:"assetId"`
	CurrentPrice float64 `json:"currentPrice"`
	Asset        Asset   `json:"asset"`
}

func NewMarketHandler(cfg *config.Config, dbConn *gorm.DB) *MarketHandler {
	return &MarketHandler{
		cfg: cfg,
		db:  dbConn,
		httpClient: &http.Client{
			Timeout: 5 * time.Second,
		},
		logger: log.Default(),
	}
}

// Llama al microservicio AssetService y obtiene la lista de activos para un team
func (h *MarketHandler) fetchAssets(ctx context.Context, teamPublicID string) ([]RemoteTeamAsset, error) {
	url := fmt.Sprintf(
		"https://assetservice-production.up.railway.app/team-assets/team/%s",
		teamPublicID,
	)

	req, err := http.NewRequestWithContext(ctx, http.MethodGet, url, nil)
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

	var teamAssets []RemoteTeamAsset
	if err := json.NewDecoder(resp.Body).Decode(&teamAssets); err != nil {
		return nil, fmt.Errorf("decodificar respuesta AssetService: %w", err)
	}

	return teamAssets, nil
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

func (h *MarketHandler) persistPrice(ctx context.Context, teamAssetID int, price float64) error {
	// 1) Insertar en historicalprices
	hp := &db.HistoricalPrice{
		Price:       price,
		TeamAssetID: teamAssetID,
	}

	if err := h.db.WithContext(ctx).Create(hp).Error; err != nil {
		return fmt.Errorf("insertar historicalprice: %w", err)
	}

	// 2) Actualizar currentprice en teamassets
	if err := h.db.WithContext(ctx).
		Model(&db.TeamAsset{}).
		Where("teamassetid = ?", teamAssetID).
		Update("currentprice", price).Error; err != nil {

		return fmt.Errorf("actualizar teamasset.currentprice: %w", err)
	}

	return nil
}

// Implementación del RPC CheckMarket
func (h *MarketHandler) CheckMarket(req *pb.MarketRequest, stream pb.MarketService_CheckMarketServer) error {
	ctx := stream.Context()

	teamID := req.GetTeamPublicId()

	remoteAssets, err := h.fetchAssets(ctx, teamID)
	if err != nil {
		return status.Errorf(codes.Internal, "no se pudo obtener activos: %v", err)
	}
	if len(remoteAssets) == 0 {
		return status.Error(codes.NotFound, "no hay activos en el sistema")
	}

	rand.Seed(time.Now().UnixNano())

	states := make([]*assetState, 0, len(remoteAssets))
	for _, ta := range remoteAssets {
		price := ta.CurrentPrice
		if price <= 0 {
			price = ta.Asset.BasePrice
		}

		states = append(states, &assetState{
			asset:        ta.Asset,
			teamAssetID:  int(ta.TeamAssetID),
			currentPrice: price,
		})
	}

	interval := h.cfg.TickInterval
	if req.GetIntervalSeconds() > 0 {
		interval = time.Duration(req.GetIntervalSeconds()) * time.Second
	}

	ticker := time.NewTicker(interval)
	defer ticker.Stop()

	for {
		select {
		case <-ctx.Done():
			return ctx.Err()

		case t := <-ticker.C:
			for _, s := range states {
				// 1) calcular nuevo precio
				s.currentPrice = nextPrice(s.currentPrice, s.asset)

				// 2) persistir en DB
				if err := h.persistPrice(ctx, s.teamAssetID, s.currentPrice); err != nil {
					// loguea pero no mates el stream por un fallo puntual
					h.logger.Printf("error guardando precio para teamAssetID=%d: %v", s.teamAssetID, err)
				}
			}

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
