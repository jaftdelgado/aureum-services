import path from 'path';
import fs from 'fs';
import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';
import mongoose from 'mongoose';
import { Video } from './models/Video'; 

// 
const MONGO_URI = "mongodb+srv://admin:admin1234@cluster0.5wusaqn.mongodb.net/trading_db?appName=Cluster0"; 

const PROTO_PATH = path.join(__dirname, 'proto', 'lecciones.proto');
const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
    keepCase: true, longs: String, enums: String, defaults: true, oneofs: true
});
const protoDescriptor = grpc.loadPackageDefinition(packageDefinition) as any;
const tradingPackage = protoDescriptor.trading;


console.log("Intentando conectar a MongoDB...");
mongoose.connect(MONGO_URI)
    .then(() => console.log("Base de Datos Conectada Exitosamente"))
    .catch(err => console.error("Error Fatal conectando a BD:", err));


const descargarVideo = async (call: any) => {
    const idSolicitado = call.request.id_leccion;
    console.log(`\n--> Cliente pide video con ID de BD: ${idSolicitado}`);

    try {
        
        const videoInfo = await Video.findById(idSolicitado);

        
        if (!videoInfo) {
            console.error(" El ID no existe en la Base de Datos");
            call.emit('error', { 
                code: grpc.status.NOT_FOUND, 
                details: "ID de video no encontrado en MongoDB" 
            });
            return;
        }

        console.log(` Encontrado en BD: "${videoInfo.descripcion}"`);
        console.log(`   Archivo físico: ${videoInfo.nombre_archivo}`);

        
        const pathVideo = path.join(__dirname, videoInfo.nombre_archivo!);

        if (!fs.existsSync(pathVideo)) {
            console.error(" GRAVE: El registro existe en BD pero el archivo mp4 no está en la carpeta");
            call.emit('error', { 
                code: grpc.status.NOT_FOUND, 
                details: "Archivo de video corrupto o perdido en servidor" 
            });
            return;
        }

       
        const videoStream = fs.createReadStream(pathVideo, { highWaterMark: 64 * 1024 });
        
        videoStream.on('data', (chunk) => {
            call.write({ contenido: chunk });
            process.stdout.write('.'); 
        });
        
        videoStream.on('end', () => {
            console.log("\nTransmisión finalizada con éxito.");
            call.end();
        });

        videoStream.on('error', (err) => {
            console.error("Error leyendo disco:", err);
            call.end();
        });

    } catch (error) {
        console.error("Error del servidor:", error);
        call.emit('error', { code: grpc.status.INTERNAL, details: "Error interno del servidor" });
    }
};


const server = new grpc.Server();
server.addService(tradingPackage.LeccionesService.service, { 
    DescargarVideo: descargarVideo 
});


const PORT_NUMBER = process.env.PORT || "50051";
const BIND_ADDRESS = `0.0.0.0:${PORT_NUMBER}`;


server.bindAsync(BIND_ADDRESS, grpc.ServerCredentials.createInsecure(), (error, port) => {
    if (error) {
        console.error("Error al iniciar gRPC:", error);
        return;
    }
    console.log(`--- Servidor gRPC escuchando en ${BIND_ADDRESS} (Puerto real: ${port}) ---`);
    
});
