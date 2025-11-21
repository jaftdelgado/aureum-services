import mongoose from 'mongoose';

const videoSchema = new mongoose.Schema({
    nombre_archivo: { type: String, required: true },
    descripcion: { type: String, required: false }
});

export const Video = mongoose.model('Video', videoSchema);