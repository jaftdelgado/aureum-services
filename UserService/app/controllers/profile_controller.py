from fastapi import APIRouter, Depends, HTTPException, status, UploadFile, File, Form
from sqlalchemy.orm import Session
from fastapi.responses import Response
from .. import mongo_db
from ..repositories import profile_repository
from ..schemas import ProfileResponseDTO, ProfileCreateDTO, ProfileUpdateDTO, ProfileBatchRequestDTO
from ..database import get_db
from bson.objectid import ObjectId
from typing import List

router = APIRouter(
    tags=["Profiles"]
)

@router.post("/batch", response_model=List[ProfileResponseDTO])
def get_profiles_batch(
    batch_data: ProfileBatchRequestDTO,
    db: Session = Depends(get_db)
):
    return profile_repository.get_profiles_by_ids(db, batch_data.profile_ids)

@router.get("/{auth_id}", response_model=ProfileResponseDTO)
def get_user_profile(auth_id: str, db: Session = Depends(get_db)):
    profile = profile_repository.get_profile_by_auth_id(db, auth_id=auth_id)
    
    if not profile:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND, 
            detail="Perfil no encontrado"
        )
    
    return profile

@router.post("", response_model=ProfileResponseDTO, status_code=status.HTTP_201_CREATED)
def register_user_profile(
    profile_data: ProfileCreateDTO, 
    db: Session = Depends(get_db)
):

    if profile_repository.get_profile_by_username(db, username=profile_data.username):
         raise HTTPException(
             status_code=status.HTTP_409_CONFLICT,
             detail="El nombre de usuario ya está en uso."
         )
         
    if profile_repository.get_profile_by_auth_id(db, auth_id=str(profile_data.auth_user_id)):
          raise HTTPException(
             status_code=status.HTTP_409_CONFLICT,
             detail="Este usuario ya tiene un perfil registrado."
         )

    try:
        new_profile = profile_repository.create_profile(db=db, profile_data=profile_data)
        return new_profile
    except Exception as e:
        db.rollback()
        print(f"Error al crear perfil: {e}") 
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Error interno al registrar el perfil."
        )

@router.patch("/{auth_id}", response_model=ProfileResponseDTO)
def update_user_profile(
    auth_id: str, 
    profile_update: ProfileUpdateDTO,
    db: Session = Depends(get_db)
):
    updated_profile = profile_repository.update_profile(db, auth_id, profile_update)
    if not updated_profile:
        raise HTTPException(status_code=404, detail="Perfil no encontrado")
    return updated_profile

@router.post("/{auth_id}/avatar", response_model=ProfileResponseDTO)
async def upload_avatar(
    auth_id: str,
    file: UploadFile = File(...),
    db: Session = Depends(get_db)
):
    profile = profile_repository.get_profile_by_auth_id(db, auth_id)
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
        
        updated_profile = profile_repository.update_profile_pic(db, auth_id, mongo_id)
        
        return updated_profile

    except Exception as e:
        print(f"Error subiendo imagen: {e}")
        raise HTTPException(status_code=500, detail="Error al subir la imagen")

@router.delete("/{auth_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete_user_profile(auth_id: str, db: Session = Depends(get_db)):
    success = profile_repository.delete_profile(db, auth_id)
    if not success:
        raise HTTPException(status_code=404, detail="Perfil no encontrado")
    return None

@router.get("/{auth_id}/avatar")
def get_avatar(auth_id: str, db: Session = Depends(get_db)):
    print(f"📷 Solicitando avatar para: {auth_id}")
    
    profile = profile_repository.get_profile_by_auth_id(db, auth_id)
    
    if not profile:
        print(" Perfil no encontrado en Postgres")
        raise HTTPException(status_code=404, detail="Perfil no encontrado")
        
    if not profile.profile_pic_id:
        print(" El perfil no tiene foto asociada (profile_pic_id es null)")
        raise HTTPException(status_code=404, detail="Avatar no configurado")

    print(f"🔍 Buscando en Mongo ID: {profile.profile_pic_id}")

    try:
        collection = mongo_db.mongo_client.get_collection()
        image_doc = collection.find_one({"_id": ObjectId(profile.profile_pic_id)})

        if not image_doc:
            print(f" Documento no encontrado en Mongo para ID: {profile.profile_pic_id}")
            raise HTTPException(status_code=404, detail="Imagen no encontrada en base de datos")

        print("✅ Imagen encontrada, enviando bytes...")
        
        return Response(
            content=image_doc["image_data"], 
            media_type=image_doc.get("content_type", "image/jpeg")
        )

    except HTTPException as e:
        raise e

    except Exception as e:
        print(f" ERROR CRÍTICO EN GET_AVATAR: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Error interno: {str(e)}")
