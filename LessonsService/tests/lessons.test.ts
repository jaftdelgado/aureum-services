import mongoose from 'mongoose';
import { Readable } from 'stream';

// 1. Definimos el esquema temporalmente aquí para la prueba
// (Idealmente exportarías esto de tu server.ts, pero para no modificar tu código actual, lo replicamos)
const lessonSchema = new mongoose.Schema({
    title: String,
    description: String,
    thumbnail: Buffer,
    videoFileId: mongoose.Types.ObjectId
});
const Lesson = mongoose.model('LessonTest', lessonSchema);

// URI: En GitHub Actions usaremos el servicio 'mongo', en local 'localhost'
const MONGO_URI = process.env.MONGO_URI || "mongodb://localhost:27017/lessons_test_db";

let gridFSBucket: mongoose.mongo.GridFSBucket;

describe('LessonsService - Integration Tests', () => {

    // --- ANTES DE TODO: CONECTAR ---
    beforeAll(async () => {
        await mongoose.connect(MONGO_URI);
        // Inicializamos el bucket de videos
        gridFSBucket = new mongoose.mongo.GridFSBucket(mongoose.connection.db!, { bucketName: 'videos' });
    });

    // --- DESPUÉS DE CADA TEST: LIMPIAR ---
    afterEach(async () => {
        const collections = mongoose.connection.collections;
        for (const key in collections) {
            await collections[key].deleteMany({});
        }
        // Limpiar archivos de GridFS es más complejo, para pruebas rápidas borramos la colección de archivos
        if (mongoose.connection.db) {
            await mongoose.connection.db.collection('videos.files').deleteMany({});
            await mongoose.connection.db.collection('videos.chunks').deleteMany({});
        }
    });

    // --- AL FINALIZAR TODO: DESCONECTAR ---
    afterAll(async () => {
        await mongoose.connection.dropDatabase();
        await mongoose.connection.close();
    });

  
    test('Debe subir un video a GridFS y guardar la Lección', async () => {
        
        const videoContent = Buffer.from("Este es un video simulado en bytes");
        const videoName = "video_prueba.mp4";
        
       
        const uploadStream = gridFSBucket.openUploadStream(videoName);
        const readable = new Readable();
        readable.push(videoContent);
        readable.push(null);
        readable.pipe(uploadStream);

        
        await new Promise((resolve, reject) => {
            uploadStream.on('finish', resolve);
            uploadStream.on('error', reject);
        });

        const videoId = uploadStream.id;

       
        const nuevaLeccion = new Lesson({
            title: "Curso de Testing",
            description: "Aprendiendo Jest con Mongo",
            videoFileId: videoId
        });
        const guardada = await nuevaLeccion.save();

        
        expect(guardada._id).toBeDefined();
        expect(guardada.title).toBe("Curso de Testing");
        expect(guardada.videoFileId).toEqual(videoId);
    });

    
    test('Debe recuperar el stream del video correctamente', async () => {
        
        const contenidoOriginal = "Bytes del video para descargar";
        const uploadStream = gridFSBucket.openUploadStream("download_test.mp4");
        const readable = new Readable();
        readable.push(Buffer.from(contenidoOriginal));
        readable.push(null);
        readable.pipe(uploadStream);
        
        await new Promise((resolve) => uploadStream.on('finish', resolve));
        const fileId = uploadStream.id;

        
        const downloadStream = gridFSBucket.openDownloadStream(fileId);
        
        const chunks: Buffer[] = [];
        for await (const chunk of downloadStream) {
            chunks.push(chunk);
        }
        
        const bufferDescargado = Buffer.concat(chunks);
        
        
        expect(bufferDescargado.toString()).toBe(contenidoOriginal);
    });
});