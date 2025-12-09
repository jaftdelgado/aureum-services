import path from 'path';
import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';
import express from 'express';
import multer from 'multer';
import cors from 'cors';
import { connectDatabase } from './config/database';
import { GrpcController } from './controllers/grpc.controller';
import { uploadLesson } from './controllers/http.controller';

/** Puerto de escucha para comunicaciones gRPC (Microservicios). */
const GRPC_PORT = "50051";

/** Puerto de escucha para peticiones HTTP/REST (Subidas de archivos). */
const HTTP_PORT = 3000;

// Configuración de Carga de Proto (gRPC)
/**
 * Carga dinámica del contrato .proto.
 * Opciones críticas:
 * - longs: String -> Para evitar pérdida de precisión con IDs de 64 bits en JS.
 * - oneofs: true -> Soporte correcto para campos opcionales.
 */
const PROTO_PATH = path.join(__dirname, 'proto', 'lecciones.proto');
const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
    keepCase: true, 
    longs: String, 
    enums: String, 
    defaults: true, 
    oneofs: true
});
const protoDescriptor = grpc.loadPackageDefinition(packageDefinition) as any;
const tradingPackage = protoDescriptor.trading;

const app = express();

/**
 * Configuración de Multer para manejo de archivos "Multipart".
 * Se usa 'memoryStorage' para mantener los archivos en RAM temporalmente.
 * Esto permite convertir el Buffer a Stream y enviarlo a GridFS (MongoDB) 
 * sin escribir archivos temporales en el disco del servidor.
 */
const upload = multer({ storage: multer.memoryStorage() });

app.use(cors());

/**
 * Ruta HTTP para la carga de lecciones.
 * Acepta: video (max 1), image (max 1).
 */
app.post('/upload', upload.fields([{ name: 'video', maxCount: 1 }, { name: 'image', maxCount: 1 }]), uploadLesson);

// Inicialización del Sistema (Bootstrapping)
/**
 * Función principal de arranque asíncrono.
 * Sigue un patrón "Fail-Fast": si la base de datos no conecta, el servidor no inicia.
 */
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
            console.log(`Servidor gRPC listo en puerto ${port}`);
        });

        app.listen(HTTP_PORT, () => {
            console.log(`API HTTP lista en http://0.0.0.0:${HTTP_PORT}`);
        });

    } catch (error) {
        console.error("Error fatal al iniciar el microservicio:", error);
        process.exit(1); 
    }
};
startServer();