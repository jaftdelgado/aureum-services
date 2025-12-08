import mongoose from 'mongoose';
import { Readable } from 'stream';
import { Lesson } from '../src/models/Lesson';
import { LessonService } from '../src/services/lesson.service';
import { connectDatabase, gridFSBucket } from '../src/config/database'; 

const TEST_MONGO_URI = "mongodb://localhost:27017/lessons_test_db";

const lessonService = new LessonService();

describe('LessonsService - Integration Tests', () => {

    // --- ANTES DE TODO: CONECTAR ---
    beforeAll(async () => {
        // Forzamos la URI de prueba en las variables de entorno antes de conectar
        process.env.MONGO_URI = TEST_MONGO_URI;
        
        // Usamos tu función de configuración real para conectar
        // Esto asegura que 'gridFSBucket' se inicialice correctamente dentro de tu app
        await connectDatabase();
    });

    // --- DESPUÉS DE CADA TEST: LIMPIAR ---
    afterEach(async () => {
        // Limpiamos la colección de lecciones
        await Lesson.deleteMany({});

        // Limpiamos los archivos de GridFS (Videos)
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

    
    test('Debe subir un video y crear la lección usando el Service', async () => {
        const videoName = "video_prueba.mp4";
        const videoContent = Buffer.from("Contenido simulado del video");

        // 1. Probamos el método getUploadStream del servicio
        const uploadStream = lessonService.getUploadStream(videoName);
        
        // Simulamos la subida (pipe)
        const readable = new Readable();
        readable.push(videoContent);
        readable.push(null);
        readable.pipe(uploadStream);

        await new Promise((resolve, reject) => {
            uploadStream.on('finish', resolve);
            uploadStream.on('error', reject);
        });

        // 2. Probamos el método createLesson del servicio
        const nuevaLeccion = await lessonService.createLesson(
            "Curso Refactorizado",
            "Descripción desde el test",
            Buffer.from("imagen_miniatura"),
            uploadStream.id
        );

        // Verificaciones
        expect(nuevaLeccion).toBeDefined();
        expect(nuevaLeccion._id).toBeDefined();
        expect(nuevaLeccion.title).toBe("Curso Refactorizado");
        // Verificamos que el ID del video se guardó correctamente
        expect(nuevaLeccion.videoFileId.toString()).toBe(uploadStream.id.toString());
    });

    
    test('Debe recuperar el stream del video usando el Service', async () => {
        // Preparación: Subimos un archivo "a mano" para tener algo que descargar
        const contenidoOriginal = "Bytes del video para descargar";
        const uploadStream = gridFSBucket.openUploadStream("download_test.mp4");
        
        const readable = new Readable();
        readable.push(Buffer.from(contenidoOriginal));
        readable.push(null);
        readable.pipe(uploadStream);
        
        await new Promise((resolve) => uploadStream.on('finish', resolve));
        const fileId = uploadStream.id;

        // --- ACT: Probamos el método getDownloadStream del servicio ---
        const downloadStream = lessonService.getDownloadStream(fileId, 0);
        
        // Leemos el stream que nos devolvió el servicio
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