# AutenticacionSeguraAA01-JC

Actividad Autónoma AA01 - Programación Web I
Instituto Superior Tecnológico Cordillera - Desarrollo de Software
Estudiante: CANDO JULIAN - Paralelo: [3AM]

## Descripción

Módulo de identidad para el sistema web de un taller de mantenimiento automotriz: registro, inicio y cierre de sesión, recuperación de contraseña por correo real, perfil, roles (Cliente y Administrador) y control de acceso. No incluye el CRUD de mantenimientos ni la página institucional de AD01.

## Tecnologías

- ASP.NET Core MVC sobre .NET [versión, ver 11.0.100]
- ASP.NET Core Identity y Entity Framework Core
- SQL Server (LocalDB o SQL Server Express)
- MailKit para el envío SMTP (Gmail)
- Bootstrap 5, Bootstrap Icons y SweetAlert2 (por CDN)

## Arquitectura de tres capas

La separación es lógica, mediante carpetas dentro de un solo proyecto.

| Capa | Carpeta | Responsabilidad |

| Presentación | `Controllers`, `Views`, `ViewModels` | Formularios, validaciones visibles, navegación según rol. Los controladores solo validan el formulario, convierten a DTO y llaman a un servicio |
| Negocio | `Negocio/Interfaces`, `Negocio/Servicios`, `Negocio/Dtos` | Reglas de registro, autenticación, recuperación, perfil y administración. Define `IEmailService` |
| Datos | `Datos` | `ContextoAplicacion`, entidad `UsuarioAplicacion`, migraciones y seeder |
| Infraestructura | `Infraestructura/Correo` | Implementación SMTP de `IEmailService` |
| Comunes | `Comunes/Constantes` | Nombres de roles compartidos |

Regla de dependencias: Presentación usa Negocio, Negocio usa Datos, e Infraestructura implementa interfaces de Negocio. Los controladores no acceden a `UserManager` ni al `DbContext`.

## Requisitos previos

- SDK de .NET [net11.0]
- SQL Server LocalDB o SQL Server Express
- Herramienta `dotnet-ef`
- Una cuenta de Gmail de prueba con verificación en dos pasos y una contraseña de aplicación

## Instalación y ejecución

1. Clonar el repositorio y entrar a la carpeta del proyecto.

   ```
   git clone [https://github.com/july-13-tech/WEB-3AM-B1-AA01-AutenticacionSegura-CandoJulian.git]
   cd [src/AutenticacionSeguraAA01-JC]
   dotnet tool restore
   cd src/[src/AutenticacionSeguraAA01-JC]
   ```

2. Configurar los secretos (User Secrets). Nunca van en `appsettings.json` ni en GitHub.


   La contraseña del administrador debe cumplir la política: mínimo 8 caracteres, con mayúscula, minúscula, número y símbolo.

3. Preparar la base de datos, con una de estas dos opciones.

   - Con migraciones:
     ```
     dotnet ef database update
     ```
   - Con el script SQL: ejecutar `Database/script.sql` en SQL Server Management Studio.

4. Ejecutar con HTTPS (la cookie de autenticación exige HTTPS).

   ```
   dotnet run --launch-profile https
   ```

5. Abrir la dirección que aparece en consola, por ejemplo `https://localhost:[7294]`. Usar `localhost` y no `127.0.0.1`, porque `AllowedHosts` está restringido.

Al iniciar, la aplicación crea automáticamente los roles `Cliente` y `Administrador` y la cuenta de administrador inicial a partir de los secretos. Esta cuenta no se crea en el script SQL. El registro público nunca asigna el rol Administrador.

## Configuración del servicio de correo

Sin secretos en el repositorio. `appsettings.json` contiene solo:

```json
"Smtp": {
  "Servidor": "smtp.gmail.com",
  "Puerto": 587,
  "NombreRemitente": "Mundo Motriz JC",
  "Usuario": "",
  "Contrasena": "",
  "CorreoRemitente": ""
}
```

- Servidor `smtp.gmail.com`, puerto 587 con STARTTLS.
- Usuario, remitente y contraseña se guardan en User Secrets.
- La contraseña es una contraseña de aplicación de Google, no la de la cuenta.


## Seguridad implementada

- Contraseñas con el `PasswordHasher` de Identity (PBKDF2 con sal e iteraciones). Nunca se guardan en texto legible.
- Política de contraseña y correo único, validados en el servidor.
- Bloqueo temporal: 5 intentos fallidos bloquean la cuenta 5 minutos. El bloqueo expira solo.
- Cookie de autenticación `HttpOnly`, `Secure` y `SameSite=Lax`, con vigencia de 7 días y expiración deslizante.
- "Recordarme": sin marcar genera una cookie de sesión; marcada genera una cookie persistente con la vigencia configurada.
- Token de recuperación generado por Identity con vigencia de 15 minutos, de un solo uso (se invalida al cambiar el sello de seguridad).
- Mensajes genéricos en el inicio de sesión y en la recuperación, para no revelar qué correos existen.
- Antiforgery automático en todos los POST.
- Política de autorización por defecto: toda ruta exige sesión salvo las marcadas con `[AllowAnonymous]`.
- Autorización por rol en servidor con `[Authorize(Roles = ...)]`, y página de Acceso denegado.
- Sesiones revalidadas en cada petición: deshabilitar una cuenta o cambiar su contraseña cierra sus sesiones abiertas.
- Protección del último administrador activo: no se puede deshabilitar ni quitarle el rol.
- Cabeceras `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy` y `Cache-Control: no-store` en páginas autenticadas.
- Secretos en User Secrets o variables de entorno.


Nota sobre "Recordarme": el comportamiento al cerrar el navegador depende también de su restauración de sesiones. Con la restauración desactivada, la cookie de sesión se elimina al cerrar y la persistente se conserva hasta su vencimiento.
