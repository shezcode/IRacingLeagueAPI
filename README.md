# API REST conectada a una BBDD
Vamos a crear una API Rest que en este caso exponga los recursos accesibles por HTTP. Para ello, cada recurso tendra una URL, 
y sobre esa URL se actuara con verbos HTTP: GET para recibir la informacion, POST para crear, PUT para actualizar y DELETE para borrar.
Cada peticion a su vez, recibe un codigo de estado para saber lo que ha pasado: 200 OK, 201 creado, 204 correcto sin contenido, 307 redireccion, 400 peticion mal formulada, 401 no autenticado, 403 sin permiso/no autorizado, 404 recurso no encontrado.
La arquitectura por capas sigue el mismo diseño que la app anterior de CLI, pero en este caso la capa de presentacion en vez tener una consola, estara compuesta por
un conjunto de controladores, y la capa de datos no persistira la info en archivos JSON, sino que utilizara un ORM (Entity Framework Core) contra una BBDD de SQL Server.
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
Para esta aplicacion tenemos dos contenedores, el de la API y el de la BBDD de Sql Server. Como son varios que tienen que arrancar a la vez, comunicarse entre si y compartir una red, usaremos Docker Compose para orquestrarlos.

El Dockerfile de la API es igual al del ej anterior, pero con la imagen de aspnet para la ejecucion en vez del runtime.
```
  FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
  WORKDIR /src
  COPY . .
  RUN dotnet publish API/App.API.csproj -c Release -o /app

  FROM mcr.microsoft.com/dotnet/aspnet:8.0
  WORKDIR /app
  COPY --from=build /app ./
  ENTRYPOINT ["dotnet", "App.API.dll"]
```

El fichero docker-compose.yml define los dos servicios sobre una misma red:
```
  services:
    db:
      image: mcr.microsoft.com/mssql/server:latest
      ports:
        - "8009:1433"
      environment:
        - mssql_sa_password=<password>
      volumes:
        - dbdata:/var/opt/msqql
      networks:
        - app-net
    api:
      build:
        context: .
        dockerfile: API/Dockerfile
      ports:
        - "9008:8080"
      environment:
        - DBConnectionString=<cadena de conexion a la bbdd>
      depends_on:
        - db
      networks:
        - app-net
    volumes:
      dbdata:

    networks:
      app-net:
```
