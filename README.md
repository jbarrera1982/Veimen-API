# Veimen API

API REST en ASP.NET Core (.NET 10) para Veimen: autenticación JWT (access + refresh), prompts de LLM, clientes, usuarios, requerimientos, consumo de tokens y procesamiento de correo.

## Docker

Requisitos: Docker Desktop (o Docker Engine) con el motor en ejecución.

1. Crear el archivo de entorno a partir del ejemplo:

   ```powershell
   Copy-Item .env.example .env
   ```

2. Completar `.env` con los valores reales. Como mínimo:

   - `CONNECTION_STRING`: cadena de conexión a MySQL.
   - `JWT_SECRET`: clave aleatoria de **al menos 32 caracteres**.

3. Construir y levantar el contenedor:

   ```powershell
   docker compose up --build -d
   ```

4. Verificar que responde:

   ```powershell
   curl http://localhost:8080/health
   docker compose logs -f veimen-api
   ```

5. Detener y eliminar el contenedor:

   ```powershell
   docker compose down
   ```

Notas:

- La API escucha en `http://localhost:8080` y el healthcheck consulta `/health` (incluye el chequeo de MySQL).
- El contenedor corre como usuario **no root** y usa `Production` como entorno por defecto.
- `.env` está excluido de git y de `.dockerignore`; nunca se copia a la imagen.
- Todas las variables disponibles y sus valores por defecto están en `.env.example`.
- Para construir la imagen sin levantarla:

  ```powershell
  docker build -t veimen-api:local .
  ```

## Publicación automática de la imagen

Cada `push` a `master` compila la imagen y la publica en **GitHub Container Registry** con el workflow `.github/workflows/docker.yml` (también se puede lanzar a mano desde **Actions → Publish Docker image → Run workflow**):

- `ghcr.io/jbarrera1982/veimen-api:latest`
- `ghcr.io/jbarrera1982/veimen-api:sha-<commit>`
- `ghcr.io/jbarrera1982/veimen-api:master`

No requiere configurar secretos: usa el `GITHUB_TOKEN` del propio repositorio (`packages: write`). La imagen se construye con Buildx y caché de GitHub Actions.

Para descargarla:

```powershell
docker pull ghcr.io/jbarrera1982/veimen-api:latest
```

> Si el paquete queda privado, hay que autenticarse antes con `docker login ghcr.io -u <usuario>` usando un token con permiso `read:packages`.

### Publicar en Docker Hub en lugar de GHCR

En `.github/workflows/docker.yml`:

1. Cambia `REGISTRY` a `docker.io` e `IMAGE_NAME` a `<usuario>/veimen-api`.
2. En el paso `Log in`, usa `username: ${{ secrets.DOCKERHUB_USERNAME }}` y `password: ${{ secrets.DOCKERHUB_TOKEN }}`.
3. Crea esos dos secretos en **Settings → Secrets and variables → Actions** (el token se genera en Docker Hub → Account settings → Security).

