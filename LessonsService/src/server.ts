import path from 'path';
import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';
import mongoose from 'mongoose';
import express from 'express';
import multer from 'multer';
import cors from 'cors';
import { Readable } from 'stream';

// Usa variables de entorno para flexibilidad en Railway
const MONGO_URI = process.env.MONGO_URI || "mongodb+srv://admin:admin1234@cluster0.5wusaqn.mongodb.net/trading_db?appName=Cluster0";
const GRPC_PORT = "50051";
// Forzamos 3000 para Express para no chocar con gRPC en Railway
const HTTP_PORT = 3000; 

const lessonSchema = new mongoose.Schema({
    title: String,
    description: String,
    thumbnail: Buffer,
    videoFileId: mongoose.Types.ObjectId
});
const Lesson = mongoose.model('Lesson', lessonSchema);

const PROTO_PATH = path.join(__dirname, 'proto', 'lecciones.proto');
const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
    keepCase: true, longs: String, enums: String, defaults: true, oneofs: true
});
const protoDescriptor = grpc.loadPackageDefinition(packageDefinition) as any;
const tradingPackage = protoDescriptor.trading;

let gridFSBucket: mongoose.mongo.GridFSBucket;
const app = express();
const upload = multer({ storage: multer.memoryStorage() });

app.use(cors());

// Endpoint HTTP para subir videos
app.post('/upload', upload.fields([{ name: 'video', maxCount: 1 }, { name: 'image', maxCount: 1 }]), async (req: any, res: any) => {
    try {
        if (!req.files || !req.files['video'] || !req.files['image']) {
            return res.status(400).send("Faltan archivos");
        }

        const videoFile = req.files['video'][0];
        const imageFile = req.files['image'][0];
        const { title, description } = req.body;

        console.log(`Subiendo video: ${videoFile.originalname}`);

        const uploadStream = gridFSBucket.openUploadStream(videoFile.originalname);
        
        const readableVideo = new Readable();
        readableVideo.push(videoFile.buffer);
        readableVideo.push(null);
        readableVideo.pipe(uploadStream);

        await new Promise((resolve, reject) => {
            uploadStream.on('finish', resolve);
            uploadStream.on('error', reject);
        });

        const nuevaLeccion = new Lesson({
            title: title || "Sin titulo",
            description: description || "Sin descripcion",
            thumbnail: imageFile.buffer,
            videoFileId: uploadStream.id
        });

        const resultado = await nuevaLeccion.save();

        console.log(`Subida exitosa. ID: ${resultado._id}`);
        res.json({ message: "Exito", id: resultado._id });

    } catch (error) {
        console.error(error);
        res.status(500).send("Error interno al subir");
    }
});

// Endpoint gRPC: Detalles
const obtenerDetalles = async (call: any, callback: any) => {
    try {
        const leccion = await Lesson.findById(call.request.id_leccion);
        if (!leccion) return callback({ code: grpc.status.NOT_FOUND });
        
        callback(null, {
            id: leccion._id.toString(),
            titulo: leccion.title,
            descripcion: leccion.description,
            miniatura: leccion.thumbnail
        });
    } catch (error) {
        callback({ code: grpc.status.INTERNAL });
    }
};

const descargarVideoGrpc = async (call: any) => {
    try {
        console.log(`Solicitando video ID: ${call.request.id_leccion}`);
        const leccion = await Lesson.findById(call.request.id_leccion);

        if (!leccion || !leccion.videoFileId) {
            console.log("Video no encontrado en BD");
            return call.end();
        }

        console.log(`Iniciando descarga de archivo: ${leccion.videoFileId}`);
        const downloadStream = gridFSBucket.openDownloadStream(leccion.videoFileId as any);

        
        downloadStream.on('data', (chunk) => {
            call.write({ contenido: chunk });
        });

        downloadStream.on('end', () => {
            console.log("Envío finalizado");
            call.end();
        });

        downloadStream.on('error', (err) => {
             console.error("Error leyendo de GridFS:", err);
             call.end();
        });

    } catch (error) {
        console.error("Error en descargarVideoGrpc:", error);
        call.end();
    }
};

const startServer = async () => {
    try {
        // 1. Conectar a Mongo y esperar
        const conn = await mongoose.connect(MONGO_URI);
        console.log("MongoDB Conectado Exitosamente");

        // 2. Inicializar GridFS solo cuando la conexión esté lista
        if (!conn.connection.db) {
             throw new Error("La base de datos no está inicializada");
        }
        gridFSBucket = new mongoose.mongo.GridFSBucket(conn.connection.db, { bucketName: 'videos' });

        // 3. Iniciar servidor gRPC
        const server = new grpc.Server();
        server.addService(tradingPackage.LeccionesService.service, { 
            ObtenerDetalles: obtenerDetalles,
            DescargarVideo: descargarVideoGrpc 
        });

        const credentials = grpc.ServerCredentials.createInsecure();

        
        
        let boundCount = 0;
        const onBind = (protocol: string, err: Error | null, port: number) => {
            if (err) {
                console.error(`Error al iniciar gRPC en ${protocol}:`, err.message);
            } else {
                console.log(` Servidor gRPC corriendo en ${protocol} puerto ${port}`);
                boundCount++;
            }
        };

        
        server.bindAsync(`0.0.0.0:${GRPC_PORT}`, credentials, (err, port) => {
            onBind("IPv4", err, port);

            
            server.bindAsync(`[::]:${GRPC_PORT}`, credentials, (err2, port2) => {
                onBind("IPv6", err2, port2);
                
                
            if (boundCount > 0) server.start();
            });
        });

        // 4. Iniciar servidor HTTP (Express)
        app.listen(HTTP_PORT, () => {
            console.log(`HTTP API corriendo en http://0.0.0.0:${HTTP_PORT}`);
        });

    } catch (error) {
        console.error("Error al iniciar la aplicación:", error);
    }
};

startServer();
