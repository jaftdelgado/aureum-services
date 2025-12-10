from fastapi import Depends, HTTPException, status, UploadFile
from sqlalchemy.orm import Session
from bson.objectid import ObjectId
from ..database import get_db
from .. import mongo_db
from ..repositories import profile_repository
from ..schemas import ProfileCreateDTO, ProfileUpdateDTO, ProfileBatchRequestDTO

class ProfileService:
    def __init__(self, db: Session = Depends(get_db)):
        self.db = db

    def get_profiles_batch(self, batch_data: ProfileBatchRequestDTO):
        return profile_repository.get_profiles_by_ids(self.db, batch_data.profile_ids)

    def get_profile_by_auth_id(self, auth_id: str):
        profile = profile_repository.get_profile_by_auth_id(self.db, auth_id=auth_id)
        if not profile:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND, 
                detail="Perfil no encontrado"
            )
        return profile

    def create_profile(self, profile_data: ProfileCreateDTO):
        if profile_repository.get_profile_by_username(self.db, username=profile_data.username):
             raise HTTPException(
                 status_code=status.HTTP_409_CONFLICT,
                 detail="El nombre de usuario ya esta en uso."
             )
        
        if profile_repository.get_profile_by_auth_id(self.db, auth_id=str(profile_data.auth_user_id)):
              raise HTTPException(
                 status_code=status.HTTP_409_CONFLICT,
                 detail="Este usuario ya tiene un perfil registrado."
             )

        try:
            return profile_repository.create_profile(db=self.db, profile_data=profile_data)
        except Exception as e:
            self.db.rollback()
            print(f"Error al crear perfil: {e}") 
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail="Error interno al registrar el perfil."
            )

    def update_profile(self, auth_id: str, profile_update: ProfileUpdateDTO):
        updated_profile = profile_repository.update_profile(self.db, auth_id, profile_update)
        if not updated_profile:
            raise HTTPException(status_code=404, detail="Perfil no encontrado")
        return updated_profile

    async def upload_avatar(self, auth_id: str, file: UploadFile):
        profile = profile_repository.get_profile_by_auth_id(self.db, auth_id)
        if not profile:
            raise HTTPException(status_code=404, detail="Usuario no encontrado")

        try:
            contents = await file.read()
            
            image_doc = {
                "auth_user_id": auth_id,
                "filename": file.filename,
                "content_type": file.content_type,
                "image_data": contents 
            }
            
            collection = mongo_db.mongo_client.get_collection()
            result = collection.insert_one(image_doc)
            mongo_id = str(result.inserted_id) 
            
            return profile_repository.update_profile_pic(self.db, auth_id, mongo_id)

        except Exception as e:
            print(f"Error subiendo imagen: {e}")
            raise HTTPException(status_code=500, detail="Error al subir la imagen")

    def delete_profile(self, auth_id: str):
        success = profile_repository.delete_profile(self.db, auth_id)
        if not success:
            raise HTTPException(status_code=404, detail="Perfil no encontrado")
        return None

    def get_avatar_content(self, auth_id: str):
        print(f"Solicitando avatar para: {auth_id}")
        profile = profile_repository.get_profile_by_auth_id(self.db, auth_id)
        
        if not profile:
            raise HTTPException(status_code=404, detail="Perfil no encontrado")
            
        if not profile.profile_pic_id:
            raise HTTPException(status_code=404, detail="Avatar no configurado")

        try:
            collection = mongo_db.mongo_client.get_collection()
            image_doc = collection.find_one({"_id": ObjectId(profile.profile_pic_id)})

            if not image_doc:
                raise HTTPException(status_code=404, detail="Imagen no encontrada en base de datos")

            print("Imagen encontrada, enviando bytes...")
            return image_doc["image_data"], image_doc.get("content_type", "image/jpeg")

        except HTTPException as e:
            raise e
        except Exception as e:
            print(f" ERROR CRITICO EN GET_AVATAR: {str(e)}")
            raise HTTPException(status_code=500, detail=f"Error interno: {str(e)}")