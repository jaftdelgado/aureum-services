from fastapi import Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer
import jwt 

oauth2_scheme = OAuth2PasswordBearer(tokenUrl="token")

def get_current_user_claims(token: str = Depends(oauth2_scheme)):
    try:
        payload = jwt.decode(token, options={"verify_signature": False})
        
        user_id = payload.get("sub")
        app_metadata = payload.get("app_metadata", {})
        role = app_metadata.get("role", "student") 
        
        if not user_id:
             raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED, detail="Token inválido")
             
        return {"id": user_id, "role": role}
        
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="No se pudo validar las credenciales",
            headers={"WWW-Authenticate": "Bearer"},
        )

def require_professor(claims: dict = Depends(get_current_user_claims)):
    if claims["role"] != "professor" and claims["role"] != "teacher": 
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Se requieren permisos de profesor"
        )
    return claims