from fastapi import FastAPI, Depends, HTTPException, status
from fastapi.staticfiles import StaticFiles

from app import schemas
from app.database import get_db
from app.dependencies.dependencies import get_current_user, oauth2_scheme
from app.controllers.auth_controller import AuthController, get_auth_controller

app = FastAPI(title="AureumAuthService")
# --- Authentication helpers ---
    
@app.post("/login", response_model=schemas.Token)
def login(
    form_data: schemas.UserLogin,
    auth_controller: AuthController = Depends(get_auth_controller)
):
    try: 
        return auth_controller.login_user(form_data.identifier, form_data.password)
    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail=str(e),
            headers={"WWW-Authenticate": "Bearer"},
        )


@app.get("/health")
def health_check():
    return {"status": "ok"}

@app.post("/register", response_model=schemas.User)
def register(
    user_data: schemas.UserCreate,
    auth_controller: AuthController = Depends(get_auth_controller)
):
    try:
        return auth_controller.register_user(user_data)
    except ValueError as e:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=str(e))
