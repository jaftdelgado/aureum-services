import path from 'path';
import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';

const PROTO_PATH = path.join(__dirname, 'proto', 'lecciones.proto');
const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
    keepCase: true, longs: String, enums: String, defaults: true, oneofs: true
});
const protoDescriptor = grpc.loadPackageDefinition(packageDefinition) as any;
const leccionesPackage = protoDescriptor.trading;

const client = new leccionesPackage.LeccionesService(
    'localhost:50051',
    grpc.credentials.createInsecure()
);

console.log("Pidiendo video (Streaming)...");


const call = client.DescargarVideo({ id_leccion: "692009a6348f04c75eedffc2" });

let tamañoTotal = 0;


call.on('data', (response: any) => {
    const chunk = response.contenido;
    tamañoTotal += chunk.length;
    process.stdout.write('📦'); 
});


call.on('end', () => {
    console.log("\n\nTransmisión finalizada.");
    console.log(`Total recibido: ${(tamañoTotal / 1024 / 1024).toFixed(2)} MB`);
});

call.on('error', (e: any) => {
    console.error("Error:", e);
});