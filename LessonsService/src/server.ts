import path from 'path';
import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';
import mongoose from 'mongoose';
import express from 'express';
import multer from 'multer';
import cors from 'cors';
import { Readable, Transform } from 'stream';

const MONGO_URI = "mongodb+srv://admin:admin1234@cluster0.5wusaqn.mongodb.net/trading_db?appName=Cluster0";
const GRPC_PORT = "50051";
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

app.get('/video/:id', async (req: any, res: any) => {
    try {
        const leccion = await Lesson.findById(req.params.id);
        if (!leccion || !leccion.videoFileId) return res.status(404).send("Video no encontrado");

        res.set('Content-Type', 'video/mp4');
        res.set('Accept-Ranges', 'bytes');

        const downloadStream = gridFSBucket.openDownloadStream(leccion.videoFileId as any);
        downloadStream.pipe(res);

    } catch (error) {
        console.error(error);
        res.status(500).send("Error stream");
    }
});

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
            console.log("Video no encontrado");
            return call.end();
        }

        const downloadStream = gridFSBucket.openDownloadStream(leccion.videoFileId as any);

        const transformer = new Transform({
            objectMode: true,
            transform(chunk, encoding, callback) {
                const videoChunkMessage = { contenido: chunk };
                this.push(videoChunkMessage);
                callback();
            }
        });

        downloadStream
            .on('error', (err) => {
                console.error("Error leyendo de Mongo:", err);
                call.end();
            })
            .pipe(transformer)
            .pipe(call); 

    } catch (error) {
        console.error("Error general:", error);
        call.end();
    }
};

const startServer = async () => {
    await mongoose.connect(MONGO_URI);
    console.log("MongoDB Conectado");
    gridFSBucket = new mongoose.mongo.GridFSBucket(mongoose.connection.db!, { bucketName: 'videos' });

    const server = new grpc.Server();
    server.addService(tradingPackage.LeccionesService.service, { 
        ObtenerDetalles: obtenerDetalles,
        DescargarVideo: descargarVideoGrpc 
    });

    const GRPC_HOST_PORT = `0.0.0.0:50051`; 

    server.bindAsync(GRPC_HOST_PORT, grpc.ServerCredentials.createInsecure(), (err, port) => {
        if (err) {
            console.error("Error al iniciar gRPC:", err);
            return;
        }
        console.log(`--- Servidor gRPC corriendo en puerto ${port} ---`);
        
        server.start(); 
    });

    app.listen(HTTP_PORT, () => {
        console.log(`HTTP API (Subidas) corriendo en http://0.0.0.0:${HTTP_PORT}`);
    });
};
startServer();
