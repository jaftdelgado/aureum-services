package handler

import (
	"bytes"
	"context"
	"encoding/json"
	"fmt"
	"log"
	"math/rand"
	"net/http"
	"strings"
	"sync"
	"time"

	"github.com/google/uuid"
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
	AssetID      string  `json:"assetId"`
	CurrentPrice float64 `json:"currentPrice"`
	Asset        Asset   `json:"asset"`
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
	asset         Asset
	teamAssetID   int
	currentPrice  float64
	assetPublicID string
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

type Membership struct {
	MembershipID int       `json:"membershipid"`
	PublicID     string    `json:"publicid"`
	TeamID       string    `json:"teamid"`
	UserID       string    `json:"userid"`
	JoinedAt     time.Time `json:"joinedat"`
}

type PortfolioTransactionRequest struct {
	UserId   string  `json:"userId"`
	AssetId  string  `json:"assetId"`
	TeamId   string  `json:"teamId"`
	Quantity float64 `json:"quantity"`
	Price    float64 `json:"price"`
	IsBuy    bool    `json:"isBuy"`
}

var (
	teamStreams = make(map[string]*TeamStream)
	mu          sync.Mutex
)

type TeamStream struct {
	States      []*assetState
	Subscribers map[pb.MarketService_CheckMarketServer]bool
	Ticker      *time.Ticker
	Cancel      context.CancelFunc
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

func (h *MarketHandler) fetchAssets(ctx context.Context, teamPublicID string) ([]RemoteTeamAsset, error) {
	base := strings.TrimRight(h.cfg.AssetServiceURL, "/")
	url := fmt.Sprintf("%s/team-assets/team/%s", base, teamPublicID)

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

	r := (rand.Float64() * 2) - 1
	change := r * vol

	newPrice := prev * (1 + change)

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
	hp := &db.HistoricalPrice{
		Price:       price,
		TeamAssetID: teamAssetID,
	}

	if err := h.db.WithContext(ctx).Create(hp).Error; err != nil {
		return fmt.Errorf("insertar historicalprice: %w", err)
	}

	if err := h.db.WithContext(ctx).
		Model(&db.TeamAsset{}).
		Where("teamassetid = ?", teamAssetID).
		Update("currentprice", price).Error; err != nil {

		return fmt.Errorf("actualizar teamasset.currentprice: %w", err)
	}

	return nil
}

func (h *MarketHandler) runGlobalTicker(ts *TeamStream, ctx context.Context) {
	for {
		select {
		case <-ctx.Done():
			return

		case t := <-ts.Ticker.C:
			for _, s := range ts.States {
				s.currentPrice = nextPrice(s.currentPrice, s.asset)
				_ = h.persistPrice(context.Background(), s.teamAssetID, s.currentPrice)
			}

			resp := &pb.MarketResponse{
				TimestampUnixMillis: t.UnixMilli(),
				Assets:              make([]*pb.MarketAsset, 0, len(ts.States)),
			}

			for _, s := range ts.States {
				resp.Assets = append(resp.Assets, &pb.MarketAsset{
					Id:         s.assetPublicID,
					Symbol:     s.asset.AssetSymbol,
					Name:       s.asset.AssetName,
					Price:      s.currentPrice,
					BasePrice:  s.asset.BasePrice,
					Volatility: s.asset.Volatility,
				})
			}

			mu.Lock()
			for subscriber := range ts.Subscribers {
				_ = subscriber.Send(resp)
			}
			mu.Unlock()
		}
	}
}

func (h *MarketHandler) CheckMarket(req *pb.MarketRequest, stream pb.MarketService_CheckMarketServer) error {
	ctx := stream.Context()
	teamID := req.GetTeamPublicId()

	mu.Lock()
	ts, exists := teamStreams[teamID]

	if !exists {
		remoteAssets, err := h.fetchAssets(ctx, teamID)
		if err != nil {
			mu.Unlock()
			return status.Errorf(codes.Internal, "no se pudo obtener activos: %v", err)
		}

		states := make([]*assetState, 0, len(remoteAssets))
		for _, ta := range remoteAssets {
			price := ta.CurrentPrice
			if price <= 0 {
				price = ta.Asset.BasePrice
			}

			states = append(states, &assetState{
				asset:         ta.Asset,
				teamAssetID:   int(ta.TeamAssetID),
				currentPrice:  price,
				assetPublicID: ta.AssetID,
			})
		}

		ticker := time.NewTicker(h.cfg.TickInterval)
		ctx2, cancel := context.WithCancel(context.Background())

		ts = &TeamStream{
			States:      states,
			Subscribers: make(map[pb.MarketService_CheckMarketServer]bool),
			Ticker:      ticker,
			Cancel:      cancel,
		}

		teamStreams[teamID] = ts

		go h.runGlobalTicker(ts, ctx2)
	}

	ts.Subscribers[stream] = true
	mu.Unlock()

	<-ctx.Done()

	mu.Lock()
	delete(ts.Subscribers, stream)

	if len(ts.Subscribers) == 0 {
		ts.Cancel()
		ts.Ticker.Stop()
		delete(teamStreams, teamID)
	}
	mu.Unlock()

	return nil
}

func (h *MarketHandler) fetchTeamMemberships(ctx context.Context, teamPublicID string) ([]Membership, error) {
	base := strings.TrimRight(h.cfg.CourseServiceURL, "/")
	url := fmt.Sprintf("%s/api/v1/memberships/course/%s", base, teamPublicID)

	req, err := http.NewRequestWithContext(ctx, http.MethodGet, url, nil)
	if err != nil {
		return nil, fmt.Errorf("crear request a CourseService: %w", err)
	}

	resp, err := h.httpClient.Do(req)
	if err != nil {
		return nil, fmt.Errorf("llamar a CourseService: %w", err)
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("CourseService devolvió status %d", resp.StatusCode)
	}

	var memberships []Membership
	if err := json.NewDecoder(resp.Body).Decode(&memberships); err != nil {
		return nil, fmt.Errorf("decodificar respuesta CourseService: %w", err)
	}

	return memberships, nil
}

func (h *MarketHandler) BuyAsset(ctx context.Context, req *pb.BuyAssetRequest) (*pb.BuyAssetResponse, error) {
	teamIDStr := req.GetTeamPublicId()
	assetIDStr := req.GetAssetPublicId()
	userIDStr := req.GetUserPublicId()
	qty := req.GetQuantity()
	price := req.GetPrice()

	if teamIDStr == "" || assetIDStr == "" || userIDStr == "" {
		return nil, status.Error(codes.InvalidArgument, "team_public_id, asset_public_id y user_public_id son obligatorios")
	}
	if qty <= 0 {
		return nil, status.Error(codes.InvalidArgument, "quantity debe ser mayor a cero")
	}
	if price <= 0 {
		return nil, status.Error(codes.InvalidArgument, "price debe ser mayor a cero")
	}

	userID, err := uuid.Parse(userIDStr)
	if err != nil {
		return nil, status.Errorf(codes.InvalidArgument, "user_public_id inválido: %v", err)
	}
	assetID, err := uuid.Parse(assetIDStr)
	if err != nil {
		return nil, status.Errorf(codes.InvalidArgument, "asset_public_id inválido: %v", err)
	}

	tx := h.db.WithContext(ctx).Begin()
	if tx.Error != nil {
		return nil, status.Errorf(codes.Internal, "no se pudo iniciar transacción: %v", tx.Error)
	}
	defer func() {
		if r := recover(); r != nil {
			tx.Rollback()
		}
	}()

	now := time.Now().UTC()

	mov := &db.Movement{
		UserID:      userID,
		AssetID:     assetID,
		Quantity:    qty,
		CreatedDate: now,
	}
	if err := tx.Create(mov).Error; err != nil {
		tx.Rollback()
		return nil, status.Errorf(codes.Internal, "no se pudo crear movement: %v", err)
	}

	trx := &db.Transaction{
		MovementID:       mov.MovementID,
		TransactionPrice: -price,
		IsBuy:            true,
		CreatedDate:      now,
	}
	if err := tx.Create(trx).Error; err != nil {
		tx.Rollback()
		return nil, status.Errorf(codes.Internal, "no se pudo crear transaction: %v", err)
	}

	if err := tx.Commit().Error; err != nil {
		return nil, status.Errorf(codes.Internal, "no se pudo confirmar transacción: %v", err)
	}

	memberships, err := h.fetchTeamMemberships(ctx, teamIDStr)
	if err != nil {
		h.logger.Printf("error obteniendo memberships para team %s: %v", teamIDStr, err)
		memberships = nil
	}

	alertMsg := fmt.Sprintf("El usuario %s compró el activo %s", userIDStr, assetIDStr)

	notifications := make([]*pb.BuyAssetNotification, 0, len(memberships))
	for _, m := range memberships {
		if m.UserID == userIDStr {
			continue
		}

		notifications = append(notifications, &pb.BuyAssetNotification{
			UserPublicId: m.UserID,
			Message:      alertMsg,
		})
	}

	resp := &pb.BuyAssetResponse{
		MovementPublicId:    mov.PublicID.String(),
		TransactionPublicId: trx.PublicID.String(),
		TransactionPrice:    -price,
		Quantity:            qty,
		Notifications:       notifications,
	}

	h.notifyPortfolioService(ctx, userIDStr, assetIDStr, teamIDStr, qty, price, true)

	return resp, nil
}

func (h *MarketHandler) SellAsset(ctx context.Context, req *pb.SellAssetRequest) (*pb.SellAssetResponse, error) {
	teamIDStr := req.GetTeamPublicId()
	assetIDStr := req.GetAssetPublicId()
	userIDStr := req.GetUserPublicId()
	qty := req.GetQuantity()
	price := req.GetPrice()

	if teamIDStr == "" || assetIDStr == "" || userIDStr == "" {
		return nil, status.Error(codes.InvalidArgument, "team_public_id, asset_public_id y user_public_id son obligatorios")
	}
	if qty <= 0 {
		return nil, status.Error(codes.InvalidArgument, "quantity debe ser mayor a cero")
	}
	if price <= 0 {
		return nil, status.Error(codes.InvalidArgument, "price debe ser mayor a cero")
	}

	userID, err := uuid.Parse(userIDStr)
	if err != nil {
		return nil, status.Errorf(codes.InvalidArgument, "user_public_id inválido: %v", err)
	}
	assetID, err := uuid.Parse(assetIDStr)
	if err != nil {
		return nil, status.Errorf(codes.InvalidArgument, "asset_public_id inválido: %v", err)
	}

	tx := h.db.WithContext(ctx).Begin()
	if tx.Error != nil {
		return nil, status.Errorf(codes.Internal, "no se pudo iniciar transacción: %v", tx.Error)
	}
	defer func() {
		if r := recover(); r != nil {
			tx.Rollback()
		}
	}()

	now := time.Now().UTC()

	mov := &db.Movement{
		UserID:      userID,
		AssetID:     assetID,
		Quantity:    qty,
		CreatedDate: now,
	}
	if err := tx.Create(mov).Error; err != nil {
		tx.Rollback()
		return nil, status.Errorf(codes.Internal, "no se pudo crear movement: %v", err)
	}

	trx := &db.Transaction{
		MovementID:       mov.MovementID,
		TransactionPrice: price,
		IsBuy:            false,
		CreatedDate:      now,
	}
	if err := tx.Create(trx).Error; err != nil {
		tx.Rollback()
		return nil, status.Errorf(codes.Internal, "no se pudo crear transaction: %v", err)
	}

	if err := tx.Commit().Error; err != nil {
		return nil, status.Errorf(codes.Internal, "no se pudo confirmar transacción: %v", err)
	}

	memberships, err := h.fetchTeamMemberships(ctx, teamIDStr)
	if err != nil {
		h.logger.Printf("error obteniendo memberships para team %s: %v", teamIDStr, err)
		memberships = nil
	}

	alertMsg := fmt.Sprintf("El usuario %s vendió el activo %s", userIDStr, assetIDStr)

	notifications := make([]*pb.BuyAssetNotification, 0, len(memberships))
	for _, m := range memberships {
		if m.UserID == userIDStr {
			continue
		}

		notifications = append(notifications, &pb.BuyAssetNotification{
			UserPublicId: m.UserID,
			Message:      alertMsg,
		})
	}

	resp := &pb.SellAssetResponse{
		MovementPublicId:    mov.PublicID.String(),
		TransactionPublicId: trx.PublicID.String(),
		TransactionPrice:    price,
		Quantity:            qty,
		Notifications:       notifications,
	}

	h.notifyPortfolioService(ctx, userIDStr, assetIDStr, teamIDStr, qty, price, false)
	return resp, nil

}

func (h *MarketHandler) notifyPortfolioService(ctx context.Context, userID, assetID, teamID string, qty, price float64, isBuy bool) {

	payload := PortfolioTransactionRequest{
		UserId:   userID,
		AssetId:  assetID,
		TeamId:   teamID,
		Quantity: qty,
		Price:    price,
		IsBuy:    isBuy,
	}

	body, err := json.Marshal(payload)
	if err != nil {
		h.logger.Printf("error serializando json para PortfolioService: %v", err)
		return
	}

	base := strings.TrimRight(h.cfg.PortfolioServiceURL, "/")
	url := fmt.Sprintf("%s/api/portfolio/transaction", base)

	req, err := http.NewRequestWithContext(ctx, http.MethodPost, url, bytes.NewBuffer(body))
	if err != nil {
		h.logger.Printf("error creando request a PortfolioService: %v", err)
		return
	}

	req.Header.Set("Content-Type", "application/json")

	resp, err := h.httpClient.Do(req)
	if err != nil {
		h.logger.Printf("error llamando a PortfolioService: %v", err)
		return
	}
	defer resp.Body.Close()

	if resp.StatusCode >= 400 {
		h.logger.Printf("PortfolioService devolvió error status %d", resp.StatusCode)
	} else {
		h.logger.Printf("PortfolioService actualizado correctamente (IsBuy=%v)", isBuy)
	}
}
