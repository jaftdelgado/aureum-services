FROM golang:1.25.4-alpine

WORKDIR /app

COPY go.mod go.sum ./
RUN go mod download

COPY . .

RUN go build -o /market-service ./market-service

EXPOSE 50051

ENV MARKET_GRPC_PORT=50051
ENV ASSET_SERVICE_URL=https://assetservice-production.up.railway.app/api/team-assets/team/{teamUUID}
ENV TICK_INTERVAL_SECONDS=4

CMD ["/market-service"]
