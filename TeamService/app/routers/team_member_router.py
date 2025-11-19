from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from uuid import UUID

from app.database import get_db
from app.services.team_member_service import TeamMemberService

router = APIRouter(
    prefix="/team-members",
    tags=["Team Members"]
)

@router.delete("/{public_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete_team_member(public_id: UUID, db: Session = Depends(get_db)):
    try:
        TeamMemberService.delete_member(db, public_id)
    except HTTPException as e:
        raise e
