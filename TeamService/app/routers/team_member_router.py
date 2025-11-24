from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.orm import Session
from uuid import UUID
from ..schemas.team_membership import JoinCourseDTO, TeamMembershipResponse
from app.database import get_db
from app.services.team_member_service import TeamMemberService

router = APIRouter(
    prefix="/api/v1/memberships",
    tags=["Memberships"]
)

@router.delete("/{public_id}", status_code=status.HTTP_204_NO_CONTENT)
def delete_team_member(public_id: UUID, db: Session = Depends(get_db)):
    try:
        TeamMemberService.delete_member(db, public_id)
    except HTTPException as e:
        raise e

@router.post("/join", response_model=TeamMembershipResponse, status_code=status.HTTP_201_CREATED)
def join_course(join_data: JoinCourseDTO, db: Session = Depends(get_db)):
    return team_member_service.join_course_by_code(db, join_data)