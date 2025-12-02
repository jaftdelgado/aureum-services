package handler

import (
	"bytes"
	"context"
	"io"
	"log"
	"net/http"
	"strings"
	"testing"
	"time"

	"github.com/DATA-DOG/go-sqlmock"
	"github.com/jaftdelgado/aureum-services/MarketGRPCService/internal/config"
	pb "github.com/jaftdelgado/aureum-services/MarketGRPCService/proto"
	"github.com/stretchr/testify/require"
	"google.golang.org/grpc"
	"google.golang.org/grpc/codes"
	"google.golang.org/grpc/status"
	"gorm.io/driver/postgres"
	"gorm.io/gorm"
	gormlogger "gorm.io/gorm/logger"
)

type roundTripperFunc func(*http.Request) (*http.Response, error)

func (f roundTripperFunc) RoundTrip(r *http.Request) (*http.Response, error) {
	return f(r)
}

func newMockHTTPClient(fn roundTripperFunc) *http.Client {
	return &http.Client{
		Transport: fn,
		Timeout:   2 * time.Second,
	}
}

func buildHTTPClientForBuyAsset() *http.Client {
	return newMockHTTPClient(func(r *http.Request) (*http.Response, error) {
		switch {
		case strings.Contains(r.URL.Path, "/team-assets/team/"):
			body := `[{
				"teamAssetId": 1,
				"teamId": "1",
				"assetId": "1",
				"currentPrice": 100.0,
				"asset": {
					"publicId": "11111111-1111-1111-1111-111111111111",
					"name": "BTC",
					"minPrice": 50,
					"maxPrice": 200
				}
			}]`

			return &http.Response{
				StatusCode: http.StatusOK,
				Body:       io.NopCloser(bytes.NewBufferString(body)),
				Header:     make(http.Header),
			}, nil

		case strings.Contains(r.URL.Path, "/api/v1/memberships/course/"):
			body := `[{
                "userId": "22222222-2222-2222-2222-222222222222",
                "courseId": "dummy",
                "role": "student"
            }]`
			return &http.Response{
				StatusCode: http.StatusOK,
				Body:       io.NopCloser(bytes.NewBufferString(body)),
				Header:     make(http.Header),
			}, nil

		case strings.Contains(r.URL.Path, "/portfolio/transactions"):
			return &http.Response{
				StatusCode: http.StatusOK,
				Body:       io.NopCloser(bytes.NewBufferString(`{"ok":true}`)),
				Header:     make(http.Header),
			}, nil
		default:
			return &http.Response{
				StatusCode: http.StatusNotFound,
				Body:       io.NopCloser(bytes.NewBufferString(`not found`)),
				Header:     make(http.Header),
			}, nil
		}
	})
}

func buildHTTPClientForCheckMarket() *http.Client {
	return newMockHTTPClient(func(r *http.Request) (*http.Response, error) {
		if strings.Contains(r.URL.Path, "/team-assets/team/") {
			body := `[{
				"teamAssetId": 1,
				"teamId": "1",
				"assetId": "1",
				"currentPrice": 120.0,
				"asset": {
					"publicId": "11111111-1111-1111-1111-111111111111",
					"name": "BTC",
					"minPrice": 50,
					"maxPrice": 200
				}
			}]`

			return &http.Response{
				StatusCode: http.StatusOK,
				Body:       io.NopCloser(bytes.NewBufferString(body)),
				Header:     make(http.Header),
			}, nil
		}

		return &http.Response{
			StatusCode: http.StatusNotFound,
			Body:       io.NopCloser(bytes.NewBufferString(`not found`)),
			Header:     make(http.Header),
		}, nil
	})
}

func newMockGormDB(t *testing.T) (*gorm.DB, sqlmock.Sqlmock) {
	sqlDB, mock, err := sqlmock.New()
	require.NoError(t, err)

	dialector := postgres.New(postgres.Config{
		Conn:                 sqlDB,
		PreferSimpleProtocol: true,
	})

	gdb, err := gorm.Open(dialector, &gorm.Config{
		SkipDefaultTransaction: true,
		Logger: gormlogger.New(
			log.New(io.Discard, "", log.LstdFlags),
			gormlogger.Config{
				LogLevel: gormlogger.Silent,
			},
		),
	})
	require.NoError(t, err)

	return gdb, mock
}

func newTestHandlerWithDeps(dbConn *gorm.DB, client *http.Client) *MarketHandler {
	cfg := &config.Config{
		AssetServiceURL:     "http://fake-asset",
		CourseServiceURL:    "http://fake-course",
		PortfolioServiceURL: "http://fake-portfolio",
		TickInterval:        10 * time.Millisecond,
		DBURL:               "postgres://fake",
		Port:                ":8080",
	}

	h := NewMarketHandler(cfg, dbConn)
	if client != nil {
		h.httpClient = client
	}

	h.logger.SetOutput(io.Discard)

	return h
}

func TestBuyAsset_InvalidQuantity(t *testing.T) {
	h := newTestHandlerWithDeps(nil, nil)
	ctx := context.Background()

	req := &pb.BuyAssetRequest{
		TeamPublicId:  "team-id",
		AssetPublicId: "asset-id",
		UserPublicId:  "user-id",
		Quantity:      0,    // inválido
		Price:         10.0, // válido
	}

	resp, err := h.BuyAsset(ctx, req)

	require.Nil(t, resp)
	require.Error(t, err)

	st, ok := status.FromError(err)
	require.True(t, ok)
	require.Equal(t, codes.InvalidArgument, st.Code())
}

func TestBuyAsset_OK(t *testing.T) {
	gdb, mock := newMockGormDB(t)

	mock.ExpectBegin()

	mock.ExpectQuery(`INSERT INTO "movements"`).
		WillReturnRows(
			sqlmock.NewRows([]string{"movementid", "publicid"}).
				AddRow(1, "11111111-1111-1111-1111-111111111111"),
		)

	mock.ExpectQuery(`INSERT INTO "transactions"`).
		WillReturnRows(
			sqlmock.NewRows([]string{"transactionid", "publicid"}).
				AddRow(2, "22222222-2222-2222-2222-222222222222"),
		)

	mock.ExpectCommit()
	client := buildHTTPClientForBuyAsset()

	h := newTestHandlerWithDeps(gdb, client)

	ctx := context.Background()

	req := &pb.BuyAssetRequest{
		TeamPublicId:  "33333333-3333-3333-3333-333333333333",
		AssetPublicId: "11111111-1111-1111-1111-111111111111",
		UserPublicId:  "44444444-4444-4444-4444-444444444444",
		Quantity:      2.5,
		Price:         100.0,
	}

	resp, err := h.BuyAsset(ctx, req)
	require.NoError(t, err)
	require.NotNil(t, resp)

	require.NotEmpty(t, resp.MovementPublicId)
	require.NotEmpty(t, resp.TransactionPublicId)
	require.Equal(t, 2.5, resp.Quantity)
	require.Equal(t, 100.0, resp.TransactionPrice)

	require.Len(t, resp.Notifications, 1)
	require.Equal(t, "22222222-2222-2222-2222-222222222222", resp.Notifications[0].UserPublicId)

	require.NoError(t, mock.ExpectationsWereMet())
}

type fakeCheckMarketStream struct {
	grpc.ServerStream
	ctx      context.Context
	messages []*pb.MarketResponse
}

func (f *fakeCheckMarketStream) Context() context.Context {
	return f.ctx
}

func (f *fakeCheckMarketStream) Send(resp *pb.MarketResponse) error {
	f.messages = append(f.messages, resp)
	return nil
}

func TestCheckMarket_OK(t *testing.T) {
	gdb, _ := newMockGormDB(t)

	h := newTestHandlerWithDeps(gdb, buildHTTPClientForCheckMarket())

	ctx, cancel := context.WithTimeout(context.Background(), 80*time.Millisecond)
	defer cancel()

	stream := &fakeCheckMarketStream{ctx: ctx}

	req := &pb.MarketRequest{
		TeamPublicId:    "33333333-3333-3333-3333-333333333333",
		IntervalSeconds: 0, // usar h.cfg.TickInterval (10ms)
	}

	err := h.CheckMarket(req, stream)

	require.Error(t, err)
	require.Equal(t, context.DeadlineExceeded, err)

	require.NotEmpty(t, stream.messages)

	for _, m := range stream.messages {
		require.NotNil(t, m)
		require.NotEmpty(t, m.Assets)
	}
}

func TestSellAsset_InvalidQuantity(t *testing.T) {
	h := newTestHandlerWithDeps(nil, nil)
	ctx := context.Background()

	req := &pb.SellAssetRequest{
		TeamPublicId:  "team-id",
		AssetPublicId: "asset-id",
		UserPublicId:  "user-id",
		Quantity:      0,    // inválido
		Price:         10.0, // válido
	}

	resp, err := h.SellAsset(ctx, req)

	require.Nil(t, resp)
	require.Error(t, err)

	st, ok := status.FromError(err)
	require.True(t, ok)
	require.Equal(t, codes.InvalidArgument, st.Code())
}

func TestSellAsset_OK(t *testing.T) {
	gdb, mock := newMockGormDB(t)

	mock.ExpectBegin()

	mock.ExpectQuery(`INSERT INTO "movements"`).
		WillReturnRows(
			sqlmock.NewRows([]string{"movementid", "publicid"}).
				AddRow(1, "11111111-1111-1111-1111-111111111111"),
		)

	mock.ExpectQuery(`INSERT INTO "transactions"`).
		WillReturnRows(
			sqlmock.NewRows([]string{"transactionid", "publicid"}).
				AddRow(2, "22222222-2222-2222-2222-222222222222"),
		)

	mock.ExpectCommit()

	client := buildHTTPClientForBuyAsset()

	h := newTestHandlerWithDeps(gdb, client)

	ctx := context.Background()

	req := &pb.SellAssetRequest{
		TeamPublicId:  "33333333-3333-3333-3333-333333333333",
		AssetPublicId: "11111111-1111-1111-1111-111111111111",
		UserPublicId:  "44444444-4444-4444-4444-444444444444",
		Quantity:      2.5,
		Price:         100.0,
	}

	resp, err := h.SellAsset(ctx, req)
	require.NoError(t, err)
	require.NotNil(t, resp)

	require.NotEmpty(t, resp.MovementPublicId)
	require.NotEmpty(t, resp.TransactionPublicId)
	require.Equal(t, 2.5, resp.Quantity)
	require.Equal(t, -100.0, resp.TransactionPrice) // venta, precio negativo

	require.Len(t, resp.Notifications, 1)
	require.Equal(t, "22222222-2222-2222-2222-222222222222", resp.Notifications[0].UserPublicId)

	require.NoError(t, mock.ExpectationsWereMet())
}
