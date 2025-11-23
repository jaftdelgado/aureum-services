#!/usr/bin/env bash
set -e

PROTO_DIR=./proto

protoc \
  --go_out=$PROTO_DIR --go_opt=paths=source_relative \
  --go-grpc_out=$PROTO_DIR --go-grpc_opt=paths=source_relative \
  $PROTO_DIR/market.proto
