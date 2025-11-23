import os
from pymongo import MongoClient
from dotenv import load_dotenv

load_dotenv()

MONGO_URI = os.getenv("MONGO_URI")
DB_NAME = "teams_db"
COLLECTION_NAME = "team_images"

class MongoDBClient:
    client: MongoClient = None
    db = None

    def connect(self):
        if not self.client:
            print("Conectando a Mongo (Teams)...")
            self.client = MongoClient(MONGO_URI, tls=True, tlsAllowInvalidCertificates=True)
            self.db = self.client[DB_NAME]

    def get_collection(self):
        if self.db is None:
            self.connect()
        return self.db[COLLECTION_NAME]

    def close(self):
        if self.client:
            self.client.close()

mongo_client = MongoDBClient()