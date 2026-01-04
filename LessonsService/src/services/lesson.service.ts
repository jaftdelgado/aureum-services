import  { Lesson } from '../models/Lesson';
import { gridFSBucket } from '../config/database';
import mongoose from 'mongoose';

export class LessonService {
    
    async createLesson(title: string, description: string, thumbnailBuffer: Buffer, videoFileId: mongoose.Types.ObjectId) {
        const newLesson = new Lesson({
            title: title || "Sin titulo",
            description: description || "Sin descripcion",
            thumbnail: thumbnailBuffer,
            videoFileId: videoFileId
        });
        return await newLesson.save();
    }

    async getAllLessons() {
        return await Lesson.find({});
    }

    async getLessonById(id: string) {
        return await Lesson.findById(id);
    }

    async getLessonFileSize(videoFileId: mongoose.Types.ObjectId): Promise<number> {
        const files = await gridFSBucket.find({ _id: videoFileId }).toArray();
        return files.length > 0 ? files[0].length : 0;
    }

    getDownloadStream(videoFileId: mongoose.Types.ObjectId, start: number, end?: number) {
        const options: any = { start };
        if (end && end > 0) options.end = end + 1;
        
        return gridFSBucket.openDownloadStream(videoFileId, options);
    }

    getUploadStream(filename: string) {
        return gridFSBucket.openUploadStream(filename);
    }
}
