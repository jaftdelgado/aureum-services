"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.Lesson = void 0;
const mongoose_1 = __importDefault(require("mongoose"));
const lessonSchema = new mongoose_1.default.Schema({
    title: String,
    description: String,
    thumbnail: Buffer,
    videoFileId: mongoose_1.default.Types.ObjectId
});
exports.Lesson = mongoose_1.default.model('Lesson', lessonSchema);
//# sourceMappingURL=Lesson.js.map