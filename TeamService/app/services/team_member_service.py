from sqlalchemy.orm import Session
from uuid import UUID
from fastapi import HTTPException, status
from ..repositories import team_repository, team_member_repository
from app.models.team_membership import TeamMembership, JoinCourseDTO

class TeamMemberService:

    @staticmethod
    def delete_member(db: Session, public_id: UUID) -> bool:

        member = (
            db.query(TeamMembership)
            .filter(TeamMembership.publicid == public_id)
            .first()
        )

        if not member:
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail=f"TeamMembership con UUID {public_id} no encontrado"
            )

        db.delete(member)
        db.commit()
        return True

def join_course_by_code(db: Session, join_data: JoinCourseDTO):
    course = team_repository.get_team_by_access_code(db, join_data.access_code)
    
    if not course:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND, 
            detail="Código de curso inválido o no existe."
        )

    if team_member_repository.is_member(db, course.team_id, join_data.user_id):
        raise HTTPException(
            status_code=status.HTTP_409_CONFLICT, 
            detail="El alumno ya está inscrito en este curso."
        )

    return team_member_repository.create_membership(db, course.team_id, join_data.user_id)