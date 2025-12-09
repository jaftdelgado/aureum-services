import mongoose from 'mongoose';

/**
 * Instancia global del bucket de GridFS.
 * Se utiliza para realizar operaciones de entrada/salida (lectura/escritura) de archivos grandes,
 * específicamente configurado para la colección de 'videos'.
 */
export let gridFSBucket: mongoose.mongo.GridFSBucket;

/**
 * Establece la conexión asíncrona con la base de datos MongoDB.
 * * Esta función realiza las siguientes acciones:
 * 1. Obtiene la URI de conexión desde las variables de entorno o usa una por defecto.
 * 2. Conecta con Mongoose.
 * 3. Inicializa el `gridFSBucket` utilizando la conexión establecida.
 * * Si la conexión falla, se captura el error y se termina el proceso de la aplicación
 * con código de salida 1 (error fatal).
 * * @returns {Promise<void>} Promesa que se resuelve cuando la conexión es exitosa.
 */
export const connectDatabase = async () => {
    try {
        const uri = process.env.MONGO_URI || "mongodb+srv://admin:admin1234@cluster0.5wusaqn.mongodb.net/trading_db?appName=Cluster0";
        const conn = await mongoose.connect(uri);
        console.log("MongoDB Conectado Exitosamente");

        if (conn.connection.db) {
            // Inicializamos el bucket apuntando a la colección 'videos'
            gridFSBucket = new mongoose.mongo.GridFSBucket(conn.connection.db, { bucketName: 'videos' });
        } else {
            throw new Error("La base de datos no se inicializó correctamente");
        }
    } catch (error) {
        console.error("Error conectando a MongoDB:", error);
        // Terminamos el proceso porque la DB es esencial para el microservicio
        process.exit(1);
    }
};