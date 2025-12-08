import { Request, Response } from 'express';
import { Readable } from 'stream';
import { LessonService } from '../services/lesson.service';

const lessonService = new LessonService();

export const uploadLesson = async (req: any, res: Response) => {
    try {
        if (!req.files || !req.files['video'] || !req.files['image']) {
            return res.status(400).send("Faltan archivos (video e imagen requeridos)");
        }

        const videoFile = req.files['video'][0];
        const imageFile = req.files['image'][0];
        const { title, description } = req.body;

        console.log(`Iniciando subida: ${videoFile.originalname}`);

        const uploadStream = lessonService.getUploadStream(videoFile.originalname);
        
        // Convertir buffer a stream
        const readableVideo = new Readable();
        readableVideo.push(videoFile.buffer);
        readableVideo.push(null);
        readableVideo.pipe(uploadStream);

        await new Promise((resolve, reject) => {
            uploadStream.on('finish', resolve);
            uploadStream.on('error', reject);
        });

        const result = await lessonService.createLesson(
            title, 
            description, 
            imageFile.buffer, 
            uploadStream.id
        );

        console.log(`Lección creada: ${result._id}`);
        res.json({ message: "Éxito", id: result._id });

    } catch (error) {
        console.error("Error en upload:", error);
        res.status(500).send("Error interno al subir");
    }
};