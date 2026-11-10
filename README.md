# API REST conectada a una BBDD
Vamos a crear una API Rest que en este caso exponga los recursos accesibles por HTTP. Para ello, cada recurso tendra una URL, 
y sobre esa URL se actuara con verbos HTTP: GET para recibir la informacion, POST para crear, PUT para actualizar y DELETE para borrar.
Cada peticion a su vez, recibe un codigo de estado para saber lo que ha pasado: 200 OK, 201 creado, 204 correcto sin contenido, 307 redireccion, 400 peticion mal formulada, 401 no autenticado, 403 sin permiso/no autorizado, 404 recurso no encontrado.
La arquitectura por capas sigue el mismo diseño que la app anterior de CLI, pero en este caso la capa de presentacion en vez tener una consola, estara compuesta por
un conjunto de controladores, y la capa de datos no persistira la info en archivos JSON, sino que utilizara Entity Framework Core 10 contra PostgreSQL 17.
  ## CAPAS
- Capa Modelos: Esta compuesta por las mismas entidades que antes, pero tambien añade el concepto de DTOs que son objetos que transfieren datos con la idea de 
  devolver la informacion justa y necesaria para resolver la peticion HTTP sin exponer directamente la totalidad de la entidad interna (evita mandar passwords y datos de uso interno)
- Capa Acceso a Datos: Con el ORM aparece la clase AppDbContext, que hereda de DbContext e implementa un DbSet por cada entidad de nuestro modelo de datos. El ORM es el encargado de traducir entre objetos (modelos) y tablas de nuestra BBDD. Usaremos un enfoque Code First para llevar a cabo las migraciones, es decir, a partir de las clases de C# generamos el esquema de la base de datos.
- Negocio: Los mismos servicios que antes, pero incluimos un AuthService que se encargara de la autenticacion, cifrando las contraseñas con SHA256, comprobando credenciales y generando los tokens JWT. 
- Presentacion: Cada recurso tiene un controlador, que se anota con [ApiController] y [Route("[controller]")], que hereda de ControllerBase y recibe por el constructor el servicio y un logger. Cada accion se corresponde con un verbo HTTP, se valida la entrada (ModelState) y devuelve el codigo de estado adecuado. La clase Program configura la inyeccion de dependencias, el DbContext, la autenticacion JWT, Swagger y el orden del middleware.

  ## RUTAS Y METODOS
- Autenticacion: POST /auth/register y /auth/login, que devuelven el token JWT al cliente.
- Catalogo: GET /artists, GET /artists/{id}, POST /artists, PUT artists/{id}, DELETE artists/{id}

Sobre esas rutas se implementa tambien la busqueda con filtrado y ordenacion por varios campos, pasados como parametros de consulta ([FromQuery]).

La autenticacion es por tokens de JWT, el usuario se registra o inicia sesion y recibe un token con el que mandar las peticiones. 
La autorizacion combina dos cosas: roles globales con [Authorize] y [Authorize(Role = "Admin")], y una comprobacion de propiedad del recurso, para que un usuario solo pueda modificar los recursos que le pertenecen, y no los de los demas. Las zonas publicas se pueden consultar sin autenticar, pero las privadas no.
Para los errores, los servicios lanzan excepciones tipadas y cada controlador las traduce a su codigo HTTP correspondiente. Los errores se registran de forma centralizada en el logger.

  ## ORQUESTRACION
Docker Compose arranca la API .NET 10 y PostgreSQL 17 en una red compartida. La API espera a que PostgreSQL pase el healthcheck y aplica las migraciones EF Core pendientes al iniciar. Los endpoints v1 permanecen disponibles.

`docker compose up --build` inicia ambos servicios. La API escucha en `http://localhost:8009`; PostgreSQL se publica en el host en el puerto `9008`.

## Secret configuration

The repository contains no usable database password or JWT signing key. The API reads `ConnectionStrings:ServerDB` and `JWT:SecretKey` from configuration. In local development, ASP.NET Core loads .NET user-secrets; in Docker and production, provide `ConnectionStrings__ServerDB` and `JWT__SecretKey` as environment variables. The signing key must contain at least 32 UTF-8 bytes or the API exits at startup.

### Local development

From the repository root, set private values in the API project's user-secrets store:

```sh
dotnet user-secrets set "ConnectionStrings:ServerDB" "Host=localhost;Port=9008;Database=IRacingLeague;Username=iracing;Password=YOUR_LOCAL_DATABASE_PASSWORD" --project API/IRacingLeague.API.csproj
dotnet user-secrets set "JWT:SecretKey" "$(openssl rand -base64 48)" --project API/IRacingLeague.API.csproj
```

Run the API with the `Development` environment (for example, `dotnet run --project API/IRacingLeague.API.csproj`). User-secrets are stored outside the repository and are not committed.

### Docker Compose

Copy `.env.example` to `.env`, replace the `CHANGE_ME` values with private values (use the same database password in both database fields), then run `docker compose up --build`. Compose loads `.env` for the PostgreSQL container and API; `.env` is ignored by Git. Use deployment secret management to provide the same two environment variables in production.
