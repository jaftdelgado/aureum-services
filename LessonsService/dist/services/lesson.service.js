"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.LessonService = void 0;
const Lesson_1 = require("../models/Lesson");
const database_1 = require("../config/database");
class LessonService {
    async createLesson(title, description, thumbnailBuffer, videoFileId) {
        const newLesson = new Lesson_1.Lesson({
            title: title || "Sin titulo",
            description: description || "Sin descripcion",
            thumbnail: thumbnailBuffer,
            videoFileId: videoFileId
        });
        return await newLesson.save();
    }
    async getAllLessons() {
        return await Lesson_1.Lesson.find({});
    }
    async getLessonById(id) {
        return await Lesson_1.Lesson.findById(id);
    }
    async getLessonFileSize(videoFileId) {
        const files = await database_1.gridFSBucket.find({ _id: videoFileId }).toArray();
        return files.length > 0 ? files[0].length : 0;
    }
    getDownloadStream(videoFileId, start, end) {
        const options = { start };
        if (end && end > 0)
            options.end = end;
        return database_1.gridFSBucket.openDownloadStream(videoFileId, options);
    }
    getUploadStream(filename) {
        return database_1.gridFSBucket.openUploadStream(filename);
    }
}
exports.LessonService = LessonService;
//# sourceMappingURL=lesson.service.js.map