import * as grpc from '@grpc/grpc-js';
import { LessonService } from '../services/lesson.service';

const lessonService = new LessonService();

export const GrpcController = {
    
    obtenerDetalles: async (call: any, callback: any) => {
        try {
            const leccion = await lessonService.getLessonById(call.request.id_leccion);
            
            if (!leccion || !leccion.videoFileId) {
                return callback({ code: grpc.status.NOT_FOUND, details: 'Lección no encontrada' });
            }

            const fileSize = await lessonService.getLessonFileSize(leccion.videoFileId as any);
            
            callback(null, {
                id: leccion._id.toString(),
                titulo: leccion.title,
                descripcion: leccion.description,
                miniatura: leccion.thumbnail,
                total_bytes: fileSize
            });
        } catch (error) {
            console.error(error);
            callback({ code: grpc.status.INTERNAL });
        }
    },

    obtenerTodas: async (call: any, callback: any) => {
        try {
            const lecciones = await lessonService.getAllLessons();
            
            const listaProto = lecciones.map((l: any) => ({
                id: l._id.toString(),
                titulo: l.title || "Sin título",
                descripcion: l.description || "",
                miniatura: l.thumbnail
            }));

            callback(null, { lecciones: listaProto });
        } catch (error) {
            console.error(error);
            callback({ code: grpc.status.INTERNAL });
        }
    },

    descargarVideo: async (call: any) => {
        try {
            const { id_leccion, start_byte, end_byte } = call.request;
            const leccion = await lessonService.getLessonById(id_leccion);

            if (!leccion || !leccion.videoFileId) {
                return call.end();
            }

            const stream = lessonService.getDownloadStream(
                leccion.videoFileId as any, 
                Number(start_byte) || 0, 
                Number(end_byte)
            );
            
            stream.on('data', (chunk) => call.write({ contenido: chunk }));
            stream.on('end', () => call.end());
            stream.on('error', (err) => {
                console.error("GridFS Error:", err);
                call.end();
            });

        } catch (error) {
            console.error("Stream Error:", error);
            call.end();
        }
    }
};