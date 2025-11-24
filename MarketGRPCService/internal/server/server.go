package server

import (
	"fmt"
	"log"
	"net"

	"github.com/jaftdelgado/aureum-services/MarketGRPCService/internal/handler"
	pb "github.com/jaftdelgado/aureum-services/MarketGRPCService/proto"
	"google.golang.org/grpc"
)

func Run(port string, h *handler.MarketHandler) error {
	lis, err := net.Listen("tcp", "0.0.0.0:50051")
	if err != nil {
		return fmt.Errorf("no se pudo abrir el puerto %s: %w", port, err)
	}

	grpcServer := grpc.NewServer()

	pb.RegisterMarketServiceServer(grpcServer, h)

	log.Printf("MarketGRPCService escuchando en :%s", port)

	if err := grpcServer.Serve(lis); err != nil {
		return fmt.Errorf("falló grpcServer.Serve: %w", err)
	}

	return nil
}
