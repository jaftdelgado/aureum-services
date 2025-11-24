from sqlalchemy.orm import Session
from ..models.team import Team
from ..models.team_membership import TeamMembership

def get_all_courses(db: Session):
    return db.query(Team).all()

def get_courses_by_professor(db: Session, professor_profile_id: int):
    return db.query(Team).filter(Team.professor_id == professor_profile_id).all()

def get_courses_by_student(db: Session, student_profile_id: int):
    return db.query(Team).join(
        TeamMembership, 
        Team.team_id == TeamMembership.team_id
    ).filter(
        TeamMembership.user_id == student_profile_id
    ).all()