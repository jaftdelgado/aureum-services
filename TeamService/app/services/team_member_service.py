from sqlalchemy.orm import Session
from uuid import UUID
from fastapi import HTTPException, status
from ..repositories import team_repository, team_member_repository
from app.models.team_membership import TeamMembership
from app.schemas.team_membership import JoinCourseDTO

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

def get_students_by_course(db: Session, team_public_id: UUID):
    team = team_repository.get_team_by_public_id(db, public_id=team_public_id)
    
    if not team:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND, 
            detail="Curso no encontrado con ese Public ID."
        )
    
    return team_member_repository.get_members_by_team_id(db, team.public_id)

def join_course_by_code(db: Session, join_data: JoinCourseDTO):
    course = team_repository.get_team_by_access_code(db, join_data.access_code)
    
    if not course:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND, 
            detail="Codigo de curso invalido o no existe."
        )

    if team_member_repository.is_member(db, course.public_id, join_data.user_id):
        raise HTTPException(
            status_code=status.HTTP_409_CONFLICT, 
            detail="El alumno ya esta inscrito en este curso."
        )

    return team_member_repository.create_membership(db, course.public_id, join_data.user_id)
