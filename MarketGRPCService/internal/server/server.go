package server

import (
	"fmt"
	"log"
	"net"
	"strings"

	"github.com/jaftdelgado/aureum-services/MarketGRPCService/internal/handler"
	pb "github.com/jaftdelgado/aureum-services/MarketGRPCService/proto"
	"google.golang.org/grpc"
)

func Run(portRaw string, h *handler.MarketHandler) error {
	port := portRaw
	if idx := strings.LastIndex(port, ":"); idx != -1 {
		port = port[idx+1:]
	}

	address := "0.0.0.0:" + port

	lis, err := net.Listen("tcp", address)
	if err != nil {
		return fmt.Errorf("no se pudo abrir el puerto %s (address: %s): %w", portRaw, address, err)
	}

	grpcServer := grpc.NewServer()

	pb.RegisterMarketServiceServer(grpcServer, h)

	log.Printf("MarketGRPCService escuchando en %s", address)

	if err := grpcServer.Serve(lis); err != nil {
		return fmt.Errorf("falló grpcServer.Serve: %w", err)
	}

	return nil
}
