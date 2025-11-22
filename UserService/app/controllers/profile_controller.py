from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from ..repositories import profile_repository
from ..schemas import ProfileResponseDTO, ProfileCreateDTO
from ..database import get_db

router = APIRouter(
    tags=["Profiles"]
)

@router.post("/", response_model=ProfileResponseDTO, status_code=status.HTTP_201_CREATED)
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