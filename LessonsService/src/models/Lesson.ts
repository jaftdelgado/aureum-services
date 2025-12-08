import mongoose from 'mongoose';

const lessonSchema = new mongoose.Schema({
    title: String,
    description: String,
    thumbnail: Buffer,
    videoFileId: mongoose.Types.ObjectId
});

export const Lesson = mongoose.model('Lesson', lessonSchema);