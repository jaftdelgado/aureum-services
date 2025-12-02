from sqlalchemy.orm import Session
from ..models.team_membership import TeamMembership
from uuid import UUID

def create_membership(db: Session, team_id: UUID, user_id: str):
    new_membership = TeamMembership(
        teamid=team_id,
        userid=user_id
    )
    db.add(new_membership)
    db.commit()
    db.refresh(new_membership)
    return new_membership

def is_member(db: Session, team_id: UUID, user_id: str):
    return db.query(TeamMembership).filter(
        TeamMembership.teamid == team_id,
        TeamMembership.userid == user_id
    ).first()

def get_members_by_team_id(db: Session, team_id: UUID):
    return db.query(TeamMembership).filter(TeamMembership.teamid == team_id).all()
