"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.GrpcController = void 0;
const grpc = __importStar(require("@grpc/grpc-js"));
const lesson_service_1 = require("../services/lesson.service");
const lessonService = new lesson_service_1.LessonService();
exports.GrpcController = {
    obtenerDetalles: async (call, callback) => {
        try {
            const leccion = await lessonService.getLessonById(call.request.id_leccion);
            if (!leccion || !leccion.videoFileId) {
                return callback({ code: grpc.status.NOT_FOUND, details: 'Lección no encontrada' });
            }
            const fileSize = await lessonService.getLessonFileSize(leccion.videoFileId);
            callback(null, {
                id: leccion._id.toString(),
                titulo: leccion.title,
                descripcion: leccion.description,
                miniatura: leccion.thumbnail,
                total_bytes: fileSize
            });
        }
        catch (error) {
            console.error(error);
            callback({ code: grpc.status.INTERNAL });
        }
    },
    obtenerTodas: async (call, callback) => {
        try {
            const lecciones = await lessonService.getAllLessons();
            const listaProto = lecciones.map((l) => ({
                id: l._id.toString(),
                titulo: l.title || "Sin título",
                descripcion: l.description || "",
                miniatura: l.thumbnail
            }));
            callback(null, { lecciones: listaProto });
        }
        catch (error) {
            console.error(error);
            callback({ code: grpc.status.INTERNAL });
        }
    },
    descargarVideo: async (call) => {
        try {
            const { id_leccion, start_byte, end_byte } = call.request;
            const leccion = await lessonService.getLessonById(id_leccion);
            if (!leccion || !leccion.videoFileId) {
                return call.end();
            }
            const stream = lessonService.getDownloadStream(leccion.videoFileId, Number(start_byte) || 0, Number(end_byte));
            stream.on('data', (chunk) => call.write({ contenido: chunk }));
            stream.on('end', () => call.end());
            stream.on('error', (err) => {
                console.error("GridFS Error:", err);
                call.end();
            });
        }
        catch (error) {
            console.error("Stream Error:", error);
            call.end();
        }
    }
};
//# sourceMappingURL=grpc.controller.js.map