import os
from pymongo import MongoClient
from dotenv import load_dotenv
import certifi

load_dotenv()

MONGO_URI = os.getenv("MONGO_URI")
DB_NAME = "users_db"
COLLECTION_NAME = "profile_images"

class MongoDBClient:
    client: MongoClient = None
    db = None

    def connect(self):
        if not self.client:
            print("Conectando a Mongo (Bullseye + Certifi)...")
            self.client = MongoClient(MONGO_URI, tlsCAFile=certifi.where())
            self.db = self.client[DB_NAME]
            
            try:
                self.client.admin.command('ping')
                print("✅ Conexión a Mongo Atlas EXITOSA")
            except Exception as e:
                print(f"❌ Error en PING a Mongo: {e}")

    def get_collection(self):
        if self.db is None:
            self.connect()
        return self.db[COLLECTION_NAME]

    def close(self):
        if self.client:
            self.client.close()

mongo_client = MongoDBClient()
