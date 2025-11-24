from sqlalchemy.orm import Session
from ..models.team import Team
from ..schemas.team import TeamCreateDTO
from ..repositories import team_repository
import uuid
import string 
import random

def generate_unique_code(length=8):
    chars = string.ascii_uppercase + string.digits
    code = ''.join(random.choices(chars, k=length))
    return code

def create_course(db: Session, course_data: TeamCreateDTO, team_pic_id: str = None):

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

def get_all_courses(db: Session):
    return team_repository.get_all_courses(db)

def get_professor_courses(db: Session, professor_id: int):
    return team_repository.get_courses_by_professor(db, professor_id)

def get_student_courses(db: Session, student_id: int):
    return team_repository.get_courses_by_student(db, student_id)