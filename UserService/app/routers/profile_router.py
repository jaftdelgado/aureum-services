from fastapi import APIRouter, Depends, status, UploadFile, File
from fastapi.responses import Response
from typing import List
from ..schemas import ProfileResponseDTO, ProfileCreateDTO, ProfileUpdateDTO, ProfileBatchRequestDTO
from ..services.profile_service import ProfileService

router = APIRouter(
    tags=["Profiles"]
)

@router.post(
    "/batch",
    response_model=List[ProfileResponseDTO],
    summary="Obtener multiples perfiles por ID",
    description="Recupera una lista de perfiles de usuario basada en una lista de IDs de autenticacion (auth_ids) proporcionados. Util para mostrar listas de miembros en el frontend.",
    responses={
        200: {
            "description": "Lista de perfiles recuperada exitosamente.",
            "content": {
                "application/json": {
                    "example": [{"auth_user_id": "user1", "username": "juanperez"}, {"auth_user_id": "user2", "username": "mariagonzalez"}]
                }
            }
        }
    }
)
def get_profiles_batch(
    batch_data: ProfileBatchRequestDTO,
    service: ProfileService = Depends()
):
    return service.get_profiles_batch(batch_data)

@router.get(
    "/{auth_id}",
    response_model=ProfileResponseDTO,
    summary="Obtener perfil de usuario",
    description="Busca y retorna la informacion publica del perfil de un usuario especifico usando su ID de autenticacion.",
    responses={
        200: {"description": "Perfil encontrado exitosamente."},
        404: {"description": "El perfil no existe para el ID proporcionado."}
    }
)
def get_user_profile(auth_id: str, service: ProfileService = Depends()):
    return service.get_profile_by_auth_id(auth_id)

@router.post(
    "",
    response_model=ProfileResponseDTO,
    status_code=status.HTTP_201_CREATED,
    summary="Registrar nuevo perfil",
    description="Crea un nuevo perfil de usuario asociado a una cuenta de autenticacion existente. Valida que el nombre de usuario y el ID de autenticacion sean unicos.",
    responses={
        201: {"description": "Perfil creado exitosamente."},
        409: {"description": "Conflicto: El nombre de usuario o el ID de autenticacion ya estan registrados."},
        500: {"description": "Error interno del servidor al procesar el registro."}
    }
)
def register_user_profile(
    profile_data: ProfileCreateDTO, 
    service: ProfileService = Depends()
):
    return service.create_profile(profile_data)

@router.patch(
    "/{auth_id}",
    response_model=ProfileResponseDTO,
    summary="Actualizar perfil",
    description="Actualiza parcialmente la informacion del perfil (biografia). Solo se modifican los campos enviados en el cuerpo de la peticion.",
    responses={
        200: {"description": "Perfil actualizado correctamente."},
        404: {"description": "No se encontro el perfil a actualizar."}
    }
)
def update_user_profile(
    auth_id: str, 
    profile_update: ProfileUpdateDTO,
    service: ProfileService = Depends()
):
    return service.update_profile(auth_id, profile_update)

@router.post(
    "/{auth_id}/avatar",
    response_model=ProfileResponseDTO,
    summary="Subir imagen de avatar",
    description="Recibe un archivo de imagen, lo almacena en MongoDB y actualiza la referencia en el perfil del usuario PostgreSQL.",
    responses={
        200: {"description": "Avatar subido y actualizado exitosamente."},
        404: {"description": "Usuario no encontrado."},
        500: {"description": "Error al procesar o guardar la imagen."}
    }
)
async def upload_avatar(
    auth_id: str,
    file: UploadFile = File(...),
    service: ProfileService = Depends()
):
    return await service.upload_avatar(auth_id, file)

@router.delete(
    "/{auth_id}",
    status_code=status.HTTP_204_NO_CONTENT,
    summary="Eliminar cuenta y perfil",
    description="Elimina permanentemente el perfil del usuario y sus datos asociados.",
    responses={
        204: {"description": "Perfil eliminado exitosamente (sin contenido de respuesta)."},
        404: {"description": "Perfil no encontrado."}
    }
)
def delete_user_profile(auth_id: str, service: ProfileService = Depends()):
    return service.delete_profile(auth_id)

@router.get(
    "/{auth_id}/avatar",
    summary="Obtener imagen de avatar",
    description="Devuelve el contenido binario de la imagen de perfil almacenada en MongoDB. Se retorna con el Content-Type correcto (image/jpeg, image/png) para ser renderizada directamente por el navegador.",
    responses={
        200: {
            "description": "Imagen retornada exitosamente.",
            "content": {"image/*": {}}
        },
        404: {"description": "El usuario no tiene avatar o no existe."},
        500: {"description": "Error interno al recuperar la imagen."}
    }
)
def get_avatar(auth_id: str, service: ProfileService = Depends()):
    image_data, content_type = service.get_avatar_content(auth_id)
    return Response(content=image_data, media_type=content_type)
