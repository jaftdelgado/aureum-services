from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from ..repositories import profile_repository
from .. import schemas
from ..database import get_db

# Creamos un router. main.py le pondrá el prefijo /api/v1/profiles
router = APIRouter(
    tags=["Profiles"] # Etiqueta para la documentación de Swagger/OpenAPI
)

@router.post("/", response_model=schemas.Profile, status_code=status.HTTP_201_CREATED)
def create_user_profile(
    profile: schemas.ProfileCreate, 
    db: Session = Depends(get_db)
):
    """
    Crea un nuevo perfil de usuario. 
    Este endpoint está diseñado para ser llamado (idealmente) por el 
    servicio de autenticación después de que una cuenta es creada.
    """
    
    # Opcional: Descomenta esto cuando crees la función en el repositorio
    # para evitar perfiles duplicados.
    # db_profile = profile_repository.get_profile_by_user_id(db, user_id=profile.user_id)
    # if db_profile:
    #     raise HTTPException(
    #         status_code=status.HTTP_400_BAD_REQUEST, 
    #         detail="Profile for this user_id already exists"
    #     )
        
    # Llama a la función del repositorio (que ya corregimos)
    # y le pasa el objeto 'profile' completo.
    return profile_repository.create_profile(db=db, profile=profile)


# (Opcional, pero muy recomendado)
# Endpoint para obtener un perfil basado en el user_id (del servicio de auth)
@router.get("/user/{user_id}", response_model=schemas.Profile)
def get_profile_by_user_id(user_id: int, db: Session = Depends(get_db)):
    """
    Obtiene un perfil de usuario usando el ID del servicio de autenticación (user_id).
    """
    
    # Necesitarás crear esta función en profile_repository.py:
    # def get_profile_by_user_id(db: Session, user_id: int):
    #     return db.query(models.Profile).filter(models.Profile.user_id == user_id).first()
        
    # db_profile = profile_repository.get_profile_by_user_id(db, user_id=user_id)
    
    # if db_profile is None:
    #     raise HTTPException(
    #         status_code=status.HTTP_404_NOT_FOUND, 
    #         detail="Profile not found"
    #     )
    # return db_profile
    
    # Temporalmente, mientras no exista la función:
    if user_id: # Solo para que el linter no se queje
        raise HTTPException(status_code=501, detail="get_profile_by_user_id not implemented yet")