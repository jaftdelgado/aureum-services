import path from 'path';
import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';
import express from 'express';
import multer from 'multer';
import cors from 'cors';
import { connectDatabase } from './config/database';
import { GrpcController } from './controllers/grpc.controller';
import { uploadLesson } from './controllers/http.controller';

const GRPC_PORT = "50051";
const HTTP_PORT = 3000;

const PROTO_PATH = path.join(__dirname, 'proto', 'lecciones.proto');
const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
    keepCase: true, longs: String, enums: String, defaults: true, oneofs: true
});
const protoDescriptor = grpc.loadPackageDefinition(packageDefinition) as any;
const tradingPackage = protoDescriptor.trading;

const app = express();
const upload = multer({ storage: multer.memoryStorage() });
app.use(cors());

app.post('/upload', upload.fields([{ name: 'video', maxCount: 1 }, { name: 'image', maxCount: 1 }]), uploadLesson);

const startServer = async () => {
    try {
        await connectDatabase();

        const server = new grpc.Server();
        server.addService(tradingPackage.LeccionesService.service, { 
            ObtenerDetalles: GrpcController.obtenerDetalles,
            DescargarVideo: GrpcController.descargarVideo,
            ObtenerTodas: GrpcController.obtenerTodas
        });

        server.bindAsync(`0.0.0.0:${GRPC_PORT}`, grpc.ServerCredentials.createInsecure(), (err, port) => {
            if (err) throw err;
            console.log(`🚀 Servidor gRPC listo en puerto ${port}`);
        });

        app.listen(HTTP_PORT, () => {
            console.log(`🌐 API HTTP lista en http://0.0.0.0:${HTTP_PORT}`);
        });

    } catch (error) {
        console.error("Error fatal al iniciar:", error);
        process.exit(1);
    }
};

startServer();