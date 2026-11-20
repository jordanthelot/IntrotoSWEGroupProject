import os

import jwt
from fastapi import Depends, HTTPException
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer


bearer = HTTPBearer(auto_error=False)

jwks_client = jwt.PyJWKClient(
    "https://player-auth.services.api.unity.com/.well-known/jwks.json",
    lifespan=8 * 60 * 60,
    timeout=10,
)


def require_player(
    credentials: HTTPAuthorizationCredentials | None = Depends(bearer),
) -> str:
    unauthorized = HTTPException(
        status_code=401,
        detail="A valid Unity player login is required.",
        headers={"WWW-Authenticate": "Bearer"},
    )

    if credentials is None:
        raise unauthorized

    expected_project = os.getenv("UNITY_PROJECT_ID", "").strip()

    if not expected_project:
        raise HTTPException(
            status_code=503,
            detail="The backend Unity project ID is not configured.",
        )

    try:
        signing_key = jwks_client.get_signing_key_from_jwt(
            credentials.credentials
        )

        claims = jwt.decode(
            credentials.credentials,
            signing_key.key,
            algorithms=["RS256"],
            issuer="https://player-auth.services.api.unity.com",
            options={
                "require": ["exp", "nbf", "sub", "project_id", "iss"],
                # Bind this Unity token to our project below.
                "verify_aud": False,
            },
        )

    except jwt.PyJWKClientConnectionError:
        raise HTTPException(
            status_code=503,
            detail="Could not reach Unity to verify the login.",
        ) from None

    except (jwt.InvalidTokenError, jwt.PyJWKClientError):
        raise unauthorized from None

    if claims["project_id"] != expected_project:
        raise HTTPException(
            status_code=403,
            detail="This login belongs to a different Unity project.",
        )

    player_id = claims["sub"]

    if not isinstance(player_id, str) or not player_id.strip():
        raise unauthorized

    return player_id
