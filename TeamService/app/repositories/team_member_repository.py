from sqlalchemy.orm import Session
from ..models.team_membership import TeamMembership

def create_membership(db: Session, team_id: int, user_id: int):
    new_membership = TeamMembership(
        teamid=team_id,
        userid=user_id
    )
    db.add(new_membership)
    db.commit()
    db.refresh(new_membership)
    return new_membership

def is_member(db: Session, team_id: int, user_id: int):
    return db.query(TeamMembership).filter(
        TeamMembership.teamid == team_id,
        TeamMembership.userid == user_id
    ).first()

def get_members_by_team_id(db: Session, team_id: int):
    return db.query(TeamMembership).filter(TeamMembership.teamid == team_id).all()