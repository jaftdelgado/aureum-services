from fastapi import APIRouter, Depends, status
from sqlalchemy.orm import Session
from uuid import UUID
from typing import List
from ..schemas.team_membership import JoinCourseDTO, TeamMembershipResponse
from app.database import get_db
from ..services import team_member_service

router = APIRouter(
    prefix="/api/v1/memberships",
    tags=["Memberships"]
)

@router.delete(
    "/teams/{team_id}/members/{student_id}", 
    status_code=status.HTTP_204_NO_CONTENT,
    summary="Eliminar miembro o salir del curso",
    description="Elimina una membresia especifica dado el ID del curso y del estudiante. Se utiliza para que un profesor elimine a un alumno o para que un alumno abandone el curso.",
    responses={
        204: {"description": "Miembro eliminado exitosamente."},
        404: {"description": "Membresia no encontrada."}
    }
)
def remove_team_member(
    team_id: UUID, 
    student_id: UUID, 
    db: Session = Depends(get_db)
):
    team_member_service.TeamMemberService.remove_student_by_ids(db, team_id, student_id)
    return

@router.post(
    "/join", 
    response_model=TeamMembershipResponse, 
    status_code=status.HTTP_201_CREATED,
    summary="Unirse a un curso",
    description="Permite a un estudiante inscribirse en un curso existente utilizando un codigo de acceso valido.",
    responses={
        201: {"description": "Inscripcion al curso exitosa."},
        404: {"description": "Codigo de curso invalido o curso no encontrado."},
        409: {"description": "El estudiante ya pertenece a este curso."}
    }
)
def join_course(join_data: JoinCourseDTO, db: Session = Depends(get_db)):
    return team_member_service.join_course_by_code(db, join_data)

@router.get(
    "/course/{team_public_id}", 
    response_model=List[TeamMembershipResponse],
    summary="Listar estudiantes del curso",
    description="Obtiene el listado completo de todos los estudiantes que se encuentran actualmente inscritos en el curso especificado.",
    responses={
        200: {"description": "Lista de estudiantes recuperada exitosamente (puede estar vacia)."}
    }
)
def get_course_students(team_public_id: UUID, db: Session = Depends(get_db)):
    return team_member_service.get_students_by_course(db, team_public_id)

@router.get(
    "/teams/{team_id}/members/{student_id}", 
    response_model=TeamMembershipResponse,
    summary="Obtener membresia especifica",
    description="Busca y retorna los detalles de la relacion (membresia) especifica entre un curso y un estudiante.",
    responses={
        200: {"description": "Membresia encontrada."},
        404: {"description": "No existe relacion entre el estudiante y el curso indicado."}
    }
)
def get_team_membership(
    team_id: UUID, 
    student_id: UUID, 
    db: Session = Depends(get_db)
):
    return team_member_service.get_membership_detail(db, team_id, student_id)