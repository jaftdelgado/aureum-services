from fastapi import APIRouter, Depends, HTTPException, status, Form, UploadFile, File
from typing import List
from sqlalchemy.orm import Session
from ..database import get_db
from ..schemas.team import TeamCreateDTO, TeamResponseDTO
from ..services import team_service
from .. import mongo_db
from uuid import UUID
from fastapi.responses import Response
from bson.objectid import ObjectId

router = APIRouter(
    prefix="/api/v1/courses",
    tags=["Courses"]
)

@router.get(
    "/{public_id}",
    response_model=TeamResponseDTO,
    summary="Obtener detalle del curso",
    description="Devuelve la informacion completa de un curso especifico buscandolo por su ID publico unico.",
    responses={
        200: {"description": "Informacion del curso recuperada exitosamente."},
        404: {"description": "No se encontro ningun curso con el ID publico proporcionado."}
    }
)
def get_course_detail(public_id: UUID, db: Session = Depends(get_db)):
    course = team_service.get_course_by_public_id(db, public_id)
    if not course:
        raise HTTPException(status_code=404, detail="Course not found")
    return course

@router.post(
    "",
    response_model=TeamResponseDTO,
    status_code=status.HTTP_201_CREATED,
    summary="Crear nuevo curso",
    description="Permite a un profesor crear un curso nuevo. Recibe los datos del curso y una imagen de portada opcional como multipart/form-data. La imagen se guarda en MongoDB y los metadatos en PostgreSQL.",
    responses={
        201: {"description": "Curso creado exitosamente con su imagen asociada."},
        400: {"description": "Datos de entrada invalidos o formato de archivo no soportado."},
        500: {"description": "Error interno al procesar la imagen o guardar en la base de datos."}
    }
)
async def create_new_course(
    name: str = Form(..., description="Nombre del curso"),
    description: str = Form(None, description="Descripcion breve del curso"),
    professor_id: str = Form(..., description="UUID del perfil del profesor creador"),
    file: UploadFile = File(..., description="Archivo de imagen para la portada del curso"),
    db: Session = Depends(get_db),
):
    mongo_id = None

    try:
        if file:
            contents = await file.read()
            image_doc = {
                "filename": file.filename,
                "content_type": file.content_type,
                "image_data": contents
            }
            collection = mongo_db.mongo_client.get_collection()
            result = collection.insert_one(image_doc)
            mongo_id = str(result.inserted_id)
            print(f"Imagen subida a Mongo con ID: {mongo_id}")

    except Exception as e:
        print(f"Error Mongo: {e}")
        raise HTTPException(status_code=500, detail="Error al guardar la imagen")

    course_data = TeamCreateDTO(
        name=name,
        description=description,
        professor_id=str(professor_id)
    )

    try:
        return team_service.create_course(db, course_data, team_pic_id=mongo_id)
    except Exception as e:
        db.rollback()
        if mongo_id:
            mongo_db.mongo_client.get_collection().delete_one({"_id": result.inserted_id})
        print(f"Error Postgres: {e}")
        raise HTTPException(status_code=500, detail="Error al crear el curso en base de datos")

@router.get(
    "",
    response_model=List[TeamResponseDTO],
    summary="Listar todos los cursos",
    description="Obtiene un listado completo de todos los cursos registrados en la plataforma.",
    responses={
        200: {"description": "Lista de cursos recuperada correctamente (puede estar vacia)."}
    }
)
def get_all(db: Session = Depends(get_db)):
    return team_service.get_all_courses(db)

@router.get(
    "/professor/{profile_id}",
    response_model=List[TeamResponseDTO],
    summary="Cursos de un profesor",
    description="Lista todos los cursos creados y administrados por un profesor especifico.",
    responses={
        200: {"description": "Lista de cursos del profesor recuperada."}
    }
)
def get_by_professor(profile_id: UUID, db: Session = Depends(get_db)):
    return team_service.get_professor_courses(db, profile_id)

@router.get(
    "/student/{profile_id}",
    response_model=List[TeamResponseDTO],
    summary="Cursos de un estudiante",
    description="Lista todos los cursos en los que un estudiante se encuentra inscrito actualmente.",
    responses={
        200: {"description": "Lista de inscripciones del estudiante recuperada."}
    }
)
def get_by_student(profile_id: UUID, db: Session = Depends(get_db)):
    return team_service.get_student_courses(db, profile_id)

@router.get(
    "/{public_id}/image",
    summary="Obtener imagen del curso",
    description="Devuelve el contenido binario de la imagen de portada del curso almacenada en MongoDB. Retorna el Content-Type adecuado para renderizar directamente en el navegador.",
    responses={
        200: {
            "description": "Imagen retornada exitosamente.",
            "content": {"image/*": {}}
        },
        404: {"description": "El curso no existe o no tiene una imagen asignada."},
        500: {"description": "Error interno al recuperar la imagen desde MongoDB."}
    }
)
def get_course_image(public_id: UUID, db: Session = Depends(get_db)):
    course = team_service.get_course_by_public_id(db, public_id)
    
    if not course:
        raise HTTPException(status_code=404, detail="Curso no encontrado")
        
    if not course.team_pic:
        raise HTTPException(status_code=404, detail="Este curso no tiene imagen asignada")

    try:
        collection = mongo_db.mongo_client.get_collection()
        image_doc = collection.find_one({"_id": ObjectId(course.team_pic)})

        if not image_doc:
            raise HTTPException(status_code=404, detail="Imagen no encontrada en Mongo")

        return Response(
            content=image_doc["image_data"], 
            media_type=image_doc.get("content_type", "image/jpeg")
        )

    except Exception as e:
        print(f"Error recuperando imagen del curso: {e}")
        raise HTTPException(status_code=500, detail="Error interno al recuperar la imagen")