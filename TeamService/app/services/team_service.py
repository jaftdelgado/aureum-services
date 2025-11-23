from sqlalchemy.orm import Session
from ..models.team import Team
from ..schemas.team import TeamCreateDTO
import uuid

def generate_unique_code(length=8):
    chars = string.ascii_uppercase + string.digits
    code = ''.join(random.choices(chars, k=length))
    return code

def create_course(db: Session, course_data: TeamCreateDTO):

    new_access_code = generate_unique_code()

    new_course = Team(
        public_id=uuid.uuid4(),
        
        name=course_data.name,
        description=course_data.description,
        professor_id=course_data.professor_id,
        access_code = new_access_code,
        team_pic=team_pic_id
    )
    
    db.add(new_course)
    db.commit()
    db.refresh(new_course)
    return new_course
