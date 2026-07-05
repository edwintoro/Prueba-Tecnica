# Registro Estudiantes — Prueba Técnica

Sistema de **registro estudiantil e inscripción de materias** con arquitectura **100% .NET 8**: microservicio backend hexagonal, API Gateway, Orquestador Shell y microfrontends Blazor WebAssembly.

**Punto de entrada:** http://localhost:5100

---

## Funcionalidades implementadas

| # | Requisito |
|---|-----------|
| 1 | Registro e inicio de sesión de estudiantes |
| 2 | Programa de créditos (máx. 3 materias, 9 créditos) |
| 3 | Inscripción en materias con reglas de negocio |
| 4 | Ver compañeros de clase por materia |
| 5 | Roles **Estudiante** y **Administrador** |
| 6 | Admin: CRUD estudiantes, profesores y materias |
| 7 | Admin: asignar materias a profesores (máx. 2 por profesor) |
| 8 | Estudiante: ver en línea registros de otros estudiantes + materias inscritas |
| 9 | Catálogo limitado a **10 materias** máximo |

---

## Roles y módulos

### Estudiante
| Módulo | Ruta | Descripción |
|--------|------|-------------|
| **Inscripciones** | `/inscripciones` | Adherirse al programa, inscribir/cancelar materias, ver compañeros |
| **Directorio** | `/directorio-estudiantes` | Consultar registros de otros estudiantes y sus materias inscritas |

### Administrador
| Módulo | Ruta | Descripción |
|--------|------|-------------|
| **Registro** | `/estudiantes` | CRUD de estudiantes |
| **Materias** | `/materias` | CRUD de materias y asignación a profesores |
| **Profesores** | `/profesores` | CRUD de profesores (con materias asignadas visibles) |

El navbar muestra el **rol** (Estudiante / Administrador) junto al nombre del usuario.

---

## Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│  Orquestador.Shell (:5100)  ←  ABRIR ESTA URL               │
│  ├── Microfrontend.Login   (:5102)  vía /mf-login           │
│  └── Microfrontend.Registro (:5101)  vía /mf-registro       │
└──────────────────────────┬──────────────────────────────────┘
                           │ HTTP
┌──────────────────────────▼──────────────────────────────────┐
│  API Gateway YARP (:5050)                                   │
└──────────────────────────┬──────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────┐
│  registro-ms / RegistroMs.Api (:5001)                       │
│  ├── Módulo Estudiantes                                     │
│  ├── Módulo Programas                                       │
│  ├── Módulo Catálogo (materias + profesores)                │
│  ├── Módulo Inscripciones                                   │
│  └── Módulo Auth (JWT + roles)                              │
└──────────────────────────┬──────────────────────────────────┘
                           │ ADO.NET
┌──────────────────────────▼──────────────────────────────────┐
│  MySQL — registro_db                                        │
└─────────────────────────────────────────────────────────────┘
```

### Estructura del repositorio

```
registro-ms/     Backend hexagonal (1 microservicio, 4 módulos + auth)
gateway/         API Gateway YARP (solo enrutamiento)
frontend/        Orquestador.Shell + Microfrontend.Login + Microfrontend.Registro
  └── Blazor.Shared/   Componentes, auth-bridge, clientes API compartidos
database/        Scripts MySQL y PostgreSQL
```

Detalle del backend: [`registro-ms/README.md`](registro-ms/README.md)

---

## Stack tecnológico

- **.NET 8** — ASP.NET Core, Blazor Server (Shell), Blazor WASM (microfrontends)
- **YARP** — Reverse proxy (Gateway + proxy de MFs en el Shell)
- **MySQL 8** — Base de datos (soporta PostgreSQL alternativo)
- **ADO.NET** — Persistencia hexagonal (sin ORM)
- **JWT** — Autenticación con roles en claims
- **MediatR** — CQRS en capa Application
- **Patrones:** Clean Architecture, Hexagonal, Microfrontends, API Gateway

---

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- **MySQL 8.x** en ejecución (puerto 3306)
- **No requiere Node.js**

---

## Base de datos

Crear la base de datos y cargar scripts **en este orden**:

```bash
mysql -u root -p < database/mysql/registro_db.sql
mysql -u root -p registro_db < database/mysql/auth_tables.sql
mysql -u root -p registro_db < database/mysql/admin_seed.sql
```

> Ajusta usuario/contraseña en `registro-ms/RegistroMs.Api/appsettings.json` si tu MySQL usa credenciales distintas.

**Scripts incluidos:**
- `registro_db.sql` — Tablas, datos seed (programa, profesores, materias)
- `auth_tables.sql` — Roles, usuarios, sesiones
- `admin_seed.sql` — Rol Administrador + usuario admin de prueba

---

## Ejecución local

Abrir **3 terminales** en la raíz del repositorio:

### Terminal 1 — Backend (puerto 5001)

```bash
dotnet run --project registro-ms/RegistroMs.Api/RegistroMs.Api.csproj
```

### Terminal 2 — API Gateway (puerto 5050)

```bash
dotnet run --project gateway/ApiGateway/ApiGateway.csproj
```

> **Nota macOS:** el puerto **5000** lo usa AirPlay Receiver. El gateway usa **5050** para evitar conflictos.

### Terminal 3 — Frontend Shell (puerto 5100)

```bash
dotnet run --project frontend/Orquestador.Shell/Orquestador.Shell.csproj
```

El Shell **inicia automáticamente** los microfrontends Login (:5102) y Registro (:5101) en modo Development.

### Abrir la aplicación

**http://localhost:5100**

---

## Puertos

| Servicio | Puerto | URL |
|----------|--------|-----|
| **Orquestador Shell** | **5100** | http://localhost:5100 ← **entrada principal** |
| Microfrontend Login | 5102 | (proxy: /mf-login) |
| Microfrontend Registro | 5101 | (proxy: /mf-registro) |
| API Gateway | **5050** | http://localhost:5050 |
| registro-ms (API) | 5001 | http://localhost:5001 |

---

## Usuarios de prueba

| Rol | Email | Contraseña | Acceso |
|-----|-------|------------|--------|
| **Administrador** | `admin@registro.edu` | `123456` | Registro, Materias, Profesores |
| **Estudiante** | `edwin.toro@gmail.com` | `123456` | Directorio, Inscripciones |

Los estudiantes también pueden **registrarse** desde la pantalla de login.

---

## Reglas de negocio

### Inscripciones (estudiante)
- Máximo **3 materias** por estudiante
- Máximo **9 créditos** en total
- No puede inscribir dos materias del **mismo profesor**
- No puede repetir la misma materia

### Catálogo (administrador)
- Máximo **10 materias** en el catálogo
- Cada profesor puede dictar máximo **2 materias**
- No se puede eliminar un profesor con materias asignadas

---

## Compilar solución completa

```bash
dotnet build RegistroMs.sln
```

---

## Solución de problemas

| Problema | Solución |
|----------|----------|
| `Failed to fetch` en login | Verificar que **gateway (:5050)** y **registro-ms (:5001)** estén corriendo |
| Pantalla pegada / versión vieja | Abrir pestaña nueva con **Ctrl+Shift+R** (hard refresh) |
| Puerto 5000 ocupado (macOS) | Es AirPlay — el proyecto usa **5050**, no 5000 |
| Error de conexión MySQL | Verificar que MySQL esté activo y la cadena en `appsettings.json` |
| "Esperando autenticación" en MF | Cerrar sesión, volver a login; reiniciar Shell |

### Reinicio rápido (macOS/Linux)

```bash
pkill -f "Orquestador.Shell"; pkill -f "Microfrontend"; pkill -f "RegistroMs.Api"; pkill -f "ApiGateway"
# Luego levantar de nuevo las 3 terminales
```

---

## API principales

| Método | Ruta | Rol |
|--------|------|-----|
| POST | `/api/auth/login` | Público |
| POST | `/api/auth/register` | Público |
| GET | `/api/estudiantes` | Estudiante, Admin |
| POST/PUT/DELETE | `/api/estudiantes` | Admin |
| GET/POST/PUT/DELETE | `/api/materias` | GET: todos · CUD: Admin |
| GET/POST/PUT/DELETE | `/api/profesores` | GET: todos · CUD: Admin |
| GET/POST/DELETE | `/api/inscripciones` | Estudiante |
| GET | `/api/programas/activo` | Estudiante |

Todas las rutas pasan por el gateway: `http://localhost:5050/api/...`

---

## Autor

**Edwin Toro** — edwintoro

Prueba técnica — Registro Estudiantes · Inter Rapidísimo
