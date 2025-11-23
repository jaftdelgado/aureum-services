import os
from pymongo import MongoClient
from dotenv import load_dotenv

load_dotenv()

MONGO_URI = os.getenv("MONGO_URI")
DB_NAME = "users_db"
COLLECTION_NAME = "profile_images"

class MongoDBClient:
    client: MongoClient = None
    db = None

    def connect(self):
        if not self.client:
            self.client = MongoClient(MONGO_URI, tls=True, tlsAllowInvalidCertificates=True)
            self.db = self.client[DB_NAME]
            print("Conectado a MongoDB Atlas")
            try:
                self.client.admin.command('ping')
                print("Conectado a MongoDB Atlas")
            except Exception as e:
                print(f"Error conectando a Mongo: {e}")

    def get_collection(self):
        if self.db is None:
            self.connect()
        return self.db[COLLECTION_NAME]

    def close(self):
        if self.client:
            self.client.close()

mongo_client = MongoDBClient()
