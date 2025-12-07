from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from uuid import UUID
from ..schemas.team_membership import JoinCourseDTO, TeamMembershipResponse
from app.database import get_db
from ..services import team_member_service
from typing import List
from uuid import UUID

router = APIRouter(
    prefix="/api/v1/memberships",
    tags=["Memberships"]
)

@router.delete("/teams/{team_id}/members/{student_id}", status_code=status.HTTP_204_NO_CONTENT,
    summary="Eliminar miembro o salir del curso",
    description="Elimina una membresia especifica. Se utiliza para que un profesor elimine a un alumno.",
    responses={
        204: {"description": "Miembro eliminado exitosamente"},
        404: {"description": "Membresia no encontrada"}
    }
)
def remove_team_member(
    team_id: UUID, 
    student_id: UUID, 
    db: Session = Depends(get_db)
):
    team_member_service.TeamMemberService.remove_student_by_ids(db, team_id, student_id)
    return

@router.post("/join", response_model=TeamMembershipResponse, status_code=status.HTTP_201_CREATED,
    summary="Unirse a un curso",
    description="Permite a un estudiante inscribirse en un curso usando un codigo de acceso.",
    responses={
        201: {"description": "Inscripcion exitosa"},
        404: {"description": "Codigo de curso invalido"},
        409: {"description": "El estudiante ya pertenece a este curso"}
    }
)
def join_course(join_data: JoinCourseDTO, db: Session = Depends(get_db)):
    return team_member_service.join_course_by_code(db, join_data)


@router.get("/course/{team_public_id}", response_model=List[TeamMembershipResponse],
    summary="Listar estudiantes del curso",
    description="Obtiene la lista de todos los estudiantes inscritos en un curso."
)
def get_course_students(team_public_id: UUID, db: Session = Depends(get_db)):
    return team_member_service.get_students_by_course(db, team_public_id)
