import path from 'path';
import * as grpc from '@grpc/grpc-js';
import * as protoLoader from '@grpc/proto-loader';

const ID_A_PROBAR = "692254f707019eb2f8662e56";

const PROTO_PATH = path.join(__dirname, 'proto', 'lecciones.proto');
const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
    keepCase: true, longs: String, enums: String, defaults: true, oneofs: true
});
const protoDescriptor = grpc.loadPackageDefinition(packageDefinition) as any;
const tradingPackage = protoDescriptor.trading;

const client = new tradingPackage.LeccionesService(
    'localhost:50051',
    grpc.credentials.createInsecure()
);

const probarTodo = () => {
    console.log(`PRUEBA 1: Obteniendo Detalles del ID: ${ID_A_PROBAR}`);

    client.ObtenerDetalles({ id_leccion: ID_A_PROBAR }, (err: any, ficha: any) => {
        if (err) {
            console.error("Error obteniendo detalles:", err.details);
            return;
        }

        console.log("FICHA RECIBIDA:");
        console.log(`   - Titulo: ${ficha.titulo}`);
        console.log(`   - Descripcion: ${ficha.descripcion}`);
        console.log(`   - Tamaño miniatura: ${ficha.miniatura.length} bytes`);

        console.log("\nPRUEBA 2: Reproduciendo Video...");
        
        const call = client.DescargarVideo({ id_leccion: ID_A_PROBAR });
        
        let tamañoTotal = 0;
        let paquetes = 0;

        call.on('data', (chunk: any) => {
            const datos = chunk.contenido;
            tamañoTotal += datos.length;
            paquetes++;
            process.stdout.write('.');
        });

        call.on('end', () => {
            console.log("\nSTREAMING FINALIZADO.");
            console.log(`   - Paquetes recibidos: ${paquetes}`);
            console.log(`   - Tamaño total: ${(tamañoTotal / 1024 / 1024).toFixed(2)} MB`);
        });

        call.on('error', (e: any) => {
            console.error("\nError en el video:", e);
        });
    });
};

probarTodo();