"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const path_1 = __importDefault(require("path"));
const grpc = __importStar(require("@grpc/grpc-js"));
const protoLoader = __importStar(require("@grpc/proto-loader"));
const express_1 = __importDefault(require("express"));
const multer_1 = __importDefault(require("multer"));
const cors_1 = __importDefault(require("cors"));
const database_1 = require("./config/database");
const grpc_controller_1 = require("./controllers/grpc.controller");
const http_controller_1 = require("./controllers/http.controller");
const GRPC_PORT = "50051";
const HTTP_PORT = 3000;
const PROTO_PATH = path_1.default.join(__dirname, 'proto', 'lecciones.proto');
const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
    keepCase: true, longs: String, enums: String, defaults: true, oneofs: true
});
const protoDescriptor = grpc.loadPackageDefinition(packageDefinition);
const tradingPackage = protoDescriptor.trading;
const app = (0, express_1.default)();
const upload = (0, multer_1.default)({ storage: multer_1.default.memoryStorage() });
app.use((0, cors_1.default)());
app.post('/upload', upload.fields([{ name: 'video', maxCount: 1 }, { name: 'image', maxCount: 1 }]), http_controller_1.uploadLesson);
const startServer = async () => {
    try {
        await (0, database_1.connectDatabase)();
        const server = new grpc.Server();
        server.addService(tradingPackage.LeccionesService.service, {
            ObtenerDetalles: grpc_controller_1.GrpcController.obtenerDetalles,
            DescargarVideo: grpc_controller_1.GrpcController.descargarVideo,
            ObtenerTodas: grpc_controller_1.GrpcController.obtenerTodas
        });
        server.bindAsync(`0.0.0.0:${GRPC_PORT}`, grpc.ServerCredentials.createInsecure(), (err, port) => {
            if (err)
                throw err;
            console.log(`🚀 Servidor gRPC listo en puerto ${port}`);
        });
        app.listen(HTTP_PORT, () => {
            console.log(`🌐 API HTTP lista en http://0.0.0.0:${HTTP_PORT}`);
        });
    }
    catch (error) {
        console.error("Error fatal al iniciar:", error);
        process.exit(1);
    }
};
startServer();
//# sourceMappingURL=server.js.map