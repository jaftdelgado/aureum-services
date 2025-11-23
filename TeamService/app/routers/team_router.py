from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from ..database import get_db
from ..schemas.team import TeamCreateDTO, TeamResponseDTO
from ..services import team_service
from .. import mongo_db

router = APIRouter(
    prefix="/api/v1/courses",
    tags=["Courses"]
)


@router.post("/", response_model=TeamResponseDTO, status_code=status.HTTP_201_CREATED)
async def create_new_course(
    name: str = Form(...),
    description: str = Form(None),
    professor_id: int = Form(...),
    file: UploadFile = File(...),
    db: Session = Depends(get_db)
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
        professor_id=professor_id
    )

    try:
        return team_service.create_course(db, course_data, team_pic_id=mongo_id)
    except Exception as e:
        db.rollback()
        print(f"Error Postgres: {e}")
        raise HTTPException(status_code=500, detail="Error al crear el curso en base de datos")