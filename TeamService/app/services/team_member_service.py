from sqlalchemy.orm import Session
from uuid import UUID
from fastapi import HTTPException, status

from app.models.team_membership import TeamMembership

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
