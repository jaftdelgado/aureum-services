from sqlalchemy.orm import Session
from ..models.team import Team
from uuid import UUID
from ..models.team_membership import TeamMembership

def get_all_courses(db: Session):
    return db.query(Team).all()

def get_courses_by_professor(db: Session, professor_profile_id: str):
    return db.query(Team).filter(Team.professor_id == professor_profile_id).all()

def get_courses_by_student(db: Session, student_profile_id: str):
    return db.query(Team).join(
        TeamMembership, 
        Team.public_id == TeamMembership.teamid
    ).filter(
        TeamMembership.userid == student_profile_id
    ).all()

def get_team_by_access_code(db: Session, access_code: str):
    return db.query(Team).filter(Team.access_code == access_code).first()

def get_team_by_id(db: Session, team_id: int):
    return db.query(Team).filter(Team.team_id == team_id).first()

def get_team_by_public_id(db: Session, public_id: UUID):
    return db.query(Team).filter(Team.public_id == public_id).first()