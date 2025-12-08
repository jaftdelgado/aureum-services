import mongoose from 'mongoose';
import { Readable } from 'stream';
import { Lesson } from '../src/models/Lesson';
import { LessonService } from '../src/services/lesson.service';
import { connectDatabase, gridFSBucket } from '../src/config/database'; 

const TEST_MONGO_URI = "mongodb://localhost:27017/lessons_test_db";

const lessonService = new LessonService();

describe('LessonsService - Integration Tests', () => {

    beforeAll(async () => {
        process.env.MONGO_URI = TEST_MONGO_URI;
        
        await connectDatabase();
    });

    afterEach(async () => {
        await Lesson.deleteMany({});

        if (mongoose.connection.db) {
            await mongoose.connection.db.collection('videos.files').deleteMany({});
            await mongoose.connection.db.collection('videos.chunks').deleteMany({});
        }
    });

    afterAll(async () => {
        await mongoose.connection.dropDatabase();
        await mongoose.connection.close();
    });

    
    test('Debe subir un video y crear la lección usando el Service', async () => {
        const videoName = "video_prueba.mp4";
        const videoContent = Buffer.from("Contenido simulado del video");

        const uploadStream = lessonService.getUploadStream(videoName);
        
        const readable = new Readable();
        readable.push(videoContent);
        readable.push(null);
        readable.pipe(uploadStream);

        await new Promise((resolve, reject) => {
            uploadStream.on('finish', resolve);
            uploadStream.on('error', reject);
        });

        const nuevaLeccion = await lessonService.createLesson(
            "Curso Refactorizado",
            "Descripción desde el test",
            Buffer.from("imagen_miniatura"),
            uploadStream.id
        );

        expect(nuevaLeccion).toBeDefined();
        expect(nuevaLeccion._id).toBeDefined();
        expect(nuevaLeccion.title).toBe("Curso Refactorizado");
        expect(nuevaLeccion.videoFileId.toString()).toBe(uploadStream.id.toString());
    });

    
    test('Debe recuperar el stream del video usando el Service', async () => {
        const contenidoOriginal = "Bytes del video para descargar";
        const uploadStream = gridFSBucket.openUploadStream("download_test.mp4");
        
        const readable = new Readable();
        readable.push(Buffer.from(contenidoOriginal));
        readable.push(null);
        readable.pipe(uploadStream);
        
        await new Promise((resolve) => uploadStream.on('finish', resolve));
        const fileId = uploadStream.id;

        const downloadStream = lessonService.getDownloadStream(fileId, 0);
        
        const chunks: Buffer[] = [];
        for await (const chunk of downloadStream) {
            chunks.push(chunk);
        }
        
        const bufferDescargado = Buffer.concat(chunks);
        
        expect(bufferDescargado.toString()).toBe(contenidoOriginal);
    });

    test('Debe obtener todas las lecciones', async () => {
        await Lesson.create({ title: "L1", description: "D1", videoFileId: new mongoose.Types.ObjectId() });
        await Lesson.create({ title: "L2", description: "D2", videoFileId: new mongoose.Types.ObjectId() });

        const lecciones = await lessonService.getAllLessons();

        expect(lecciones).toHaveLength(2);
        expect(lecciones[0].title).toBeDefined();
    });
});