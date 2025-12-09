import mongoose from 'mongoose';

/**
 * Esquema de base de datos para la entidad Lección.
 * Define la estructura de los metadatos asociados a un video educativo.
 */
const lessonSchema = new mongoose.Schema({
    /**
     * Título principal de la lección.
     * @type {String}
     */
    title: String,
    /**
     * Descripción textual o resumen del contenido de la lección.
     * @type {String}
     */
    description: String,
    /**
     * Imagen de portada (miniatura) almacenada directamente en el documento como datos binarios (Buffer).
     * Nota: Se recomienda mantener un tamaño pequeño para no impactar el rendimiento de la consulta.
     * @type {Buffer}
     */
    thumbnail: Buffer,
    /**
     * Referencia (Foreign Key virtual) al archivo de video almacenado en GridFS.
     * Este ObjectId apunta al `_id` del archivo en la colección `videos.files`.
     * @type {mongoose.Types.ObjectId}
     */
    videoFileId: mongoose.Types.ObjectId
});
/**
 * Modelo compilado de Mongoose para interactuar con la colección 'lessons'.
 * Permite realizar operaciones CRUD (Crear, Leer, Actualizar, Borrar).
 */
export const Lesson = mongoose.model('Lesson', lessonSchema);