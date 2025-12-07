from sqlalchemy.orm import Session
from ..models.team_membership import TeamMembership
from uuid import UUID
from app.models.team import Team

def create_membership(db: Session, team_id: UUID, user_id: str):
    new_membership = TeamMembership(
        teamid=team_id,
        userid=str(user_id)
    )
    db.add(new_membership)
    db.commit()
    db.refresh(new_membership)
    return new_membership

def is_member(db: Session, team_id: UUID, user_id: str):
    return db.query(TeamMembership).filter(
        TeamMembership.teamid == team_id,
        TeamMembership.userid == str(user_id)
    ).first()

def get_members_by_team_id(db: Session, team_id: UUID):
    return db.query(TeamMembership).filter(TeamMembership.teamid == team_id).all()

def get_by_team_and_student(db: Session, team_public_id: UUID, user_id: UUID):
    return (
        db.query(TeamMembership)
        .join(Team, Team.team_id == TeamMembership.teamid) 
        .filter(Team.public_id == team_public_id)
        .filter(TeamMembership.userid == str(user_id)) 
        .first()
    )
