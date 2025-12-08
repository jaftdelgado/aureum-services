import mongoose from 'mongoose';

export let gridFSBucket: mongoose.mongo.GridFSBucket;
export const connectDatabase = async () => {
    try {
        const uri = process.env.MONGO_URI || "mongodb+srv://admin:admin1234@cluster0.5wusaqn.mongodb.net/trading_db?appName=Cluster0";
        const conn = await mongoose.connect(uri);
        console.log("MongoDB Conectado Exitosamente");

        if (conn.connection.db) {
            gridFSBucket = new mongoose.mongo.GridFSBucket(conn.connection.db, { bucketName: 'videos' });
        } else {
            throw new Error("La base de datos no se inicializó correctamente");
        }
    } catch (error) {
        console.error("Error conectando a MongoDB:", error);
        process.exit(1);
    }
};