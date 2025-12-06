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

@router.get("/{public_id}", response_model=TeamResponseDTO,
    summary="Obtener detalle del curso",
    description="Devuelve la informacion completa de un curso especifico por su ID publico.",
    responses={404: {"description": "Curso no encontrado"}}
)
def get_course_detail(public_id: UUID, db: Session = Depends(get_db)):
    course = team_service.get_course_by_public_id(db, public_id)
    if not course:
        raise HTTPException(status_code=404, detail="Course not found")
    return course

@router.post("", response_model=TeamResponseDTO, status_code=status.HTTP_201_CREATED,
    summary="Crear nuevo curso",
    description="Permite a un profesor crear un curso nuevo, incluyendo la subida de una imagen de portada.",
    responses={
        201: {"description": "Curso creado exitosamente"},
        500: {"description": "Error al procesar la imagen o guardar en BD"}
    }
)
async def create_new_course(
    name: str = Form(...),
    description: str = Form(None),
    professor_id: str = Form(...),
    file: UploadFile = File(...),
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
        mongo_db.mongo_client.get_collection().delete_one({"_id": result.inserted_id})
        print(f"Error Postgres: {e}")
        raise HTTPException(status_code=500, detail="Error al crear el curso en base de datos")

@router.get("", response_model=List[TeamResponseDTO],
    summary="Listar todos los cursos",
    description="Obtiene un listado de todos los cursos disponibles en la plataforma."
)
def get_all(db: Session = Depends(get_db)):
    return team_service.get_all_courses(db)

@router.get("/professor/{profile_id}", response_model=List[TeamResponseDTO],
    summary="Cursos de un profesor",
    description="Lista todos los cursos creados por un profesor especifico."
)
def get_by_professor(profile_id: UUID, db: Session = Depends(get_db)):
    return team_service.get_professor_courses(db, profile_id)

@router.get("/student/{profile_id}", response_model=List[TeamResponseDTO],
    summary="Cursos de un estudiante",
    description="Lista todos los cursos en los que un estudiante esta inscrito."
)
def get_by_student(profile_id: UUID, db: Session = Depends(get_db)):
    return team_service.get_student_courses(db, profile_id)

@router.get("/{public_id}/image",
    summary="Obtener imagen del curso",
    description="Sirve la imagen de portada del curso directamente desde MongoDB.",
    responses={
        200: {"description": "Imagen retornada", "content": {"image/jpeg": {}}},
        404: {"description": "Curso o imagen no encontrada"}
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