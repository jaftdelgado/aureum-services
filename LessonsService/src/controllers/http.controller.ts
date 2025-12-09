import { Request, Response } from 'express';
import { Readable } from 'stream';
import { LessonService } from '../services/lesson.service';

const lessonService = new LessonService();

/**
 * Maneja la creación y subida de una nueva lección educativa a través de HTTP POST.
 * Procesa una solicitud `multipart/form-data` que debe incluir archivos binarios y metadatos.
 * * **Flujo del proceso:**
 * 1. Valida la recepción de los archivos obligatorios ('video' e 'image').
 * 2. Inicializa un flujo de escritura (UploadStream) hacia GridFS en MongoDB.
 * 3. Convierte el buffer del video (en memoria) a un Readable Stream.
 * 4. Transfiere (pipe) los datos del video a la base de datos.
 * 5. Una vez finalizada la carga del video, guarda el documento de la lección con los metadatos.
 * * @param {any} req - Objeto Request de Express. Se espera que contenga:
 * - `req.files['video']`: Array con el archivo de video.
 * - `req.files['image']`: Array con la imagen de miniatura.
 * - `req.body`: Objeto con `title` y `description`.
 * @param {Response} res - Objeto Response de Express utilizado para devolver el resultado.
 * @returns {Promise<Response>} Retorna un JSON con el ID de la lección creada o un código de error.
 */
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