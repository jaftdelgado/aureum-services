import * as grpc from '@grpc/grpc-js';
import { LessonService } from '../services/lesson.service';

const lessonService = new LessonService();

/**
 * Controlador que implementa los métodos definidos en el archivo .proto (Protocol Buffers).
 * Se encarga de la comunicación gRPC entre el cliente (API Gateway u otro servicio) y este microservicio.
 */
export const GrpcController = {
    
    /**
     * Recupera los detalles completos de una lección específica.
     * * @param {any} call - Objeto de llamada gRPC.
     * @param {string} call.request.id_leccion - ID de la lección a buscar.
     * @param {any} callback - Función de retorno estándar de gRPC (error, respuesta).
     * * @returns {void} Retorna un objeto con la metadata de la lección y el tamaño del video en bytes.
     * Si no se encuentra, devuelve un error gRPC `NOT_FOUND`.
     */
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

    /**
     * Obtiene una lista de todas las lecciones disponibles en la base de datos.
     * Mapea los documentos de MongoDB al formato de mensaje repetido definido en el .proto.
     * * @param {any} call - Objeto de llamada gRPC (generalmente vacío para esta solicitud).
     * @param {any} callback - Función de retorno (error, { lecciones: [...] }).
     */
    obtenerTodas: async (call: any, callback: any) => {
        try {
            const lecciones = await lessonService.getAllLessons();
            
            // Mapeo manual para asegurar que coincida con la definición del .proto
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

    /**
     * Transmite (Stream) el contenido del archivo de video almacenado en GridFS.
     * Soporta "Range Requests" (streaming parcial) para permitir adelantar/retroceder en el reproductor de video.
     * * @param {any} call - Stream de escritura gRPC.
     * @param {string} call.request.id_leccion - ID de la lección.
     * @param {number} [call.request.start_byte] - Byte de inicio para el streaming (opcional).
     * @param {number} [call.request.end_byte] - Byte final para el streaming (opcional).
     * * @description
     * 1. Verifica la existencia de la lección y el archivo de video.
     * 2. Crea un Readable Stream desde GridFS usando los bytes solicitados.
     * 3. Envía chunks de datos a través de `call.write()`.
     * 4. Finaliza la transmisión con `call.end()`.
     */
    descargarVideo: async (call: any) => {
        try {
            const { id_leccion, start_byte, end_byte } = call.request;
            const leccion = await lessonService.getLessonById(id_leccion);

            if (!leccion || !leccion.videoFileId) {
                return call.end();
            }
            const streamOptions = {
            start: Number(start_byte) || 0,
            end: Number(end_byte) || undefined,
            revision: 0 
        };
            const stream = lessonService.getDownloadStream(
                leccion.videoFileId as any, 
                 streamOptions.start, 
            streamOptions.end
            );
            const BATCH_SIZE = 512 * 1024; 
        let buffer: Buffer = Buffer.alloc(0);
          
            stream.on('data', (chunk: Buffer) => {
            buffer = Buffer.concat([buffer, chunk]);

            if (buffer.length >= BATCH_SIZE) {
                const keepGoing = call.write({ contenido: buffer });

                  if (!keepGoing) {
                    stream.pause();
                    call.once('drain', () => stream.resume());
                }
                
                buffer = Buffer.alloc(0);
            }
        });
             stream.on('end', () => {
            if (buffer.length > 0) {
                call.write({ contenido: buffer });
            }
            call.end();
        });

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
