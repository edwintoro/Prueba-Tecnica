# registro-ms — Microservicio de Registro Estudiantil

Backend con **arquitectura hexagonal (Ports & Adapters)** y **Clean Architecture**.

> El **gateway** vive en `/gateway` — separado, sin capas hexagonales.
> Solo enruta tráfico (patrón API Gateway).

## Un microservicio, módulos internos

`registro-ms` es **un solo despliegue** (`RegistroMs.Api`, puerto **5001**).  
Dentro tiene **4 bounded contexts** como módulos hexagonales:

```
registro-ms/
├── RegistroMs.Api/          ← Host único (controllers REST + composición DI)
├── BuildingBlocks/          # Kernel compartido (Result, IDbConnectionFactory)
└── Services/
    ├── Estudiantes/         # Módulo: CRUD estudiantes
    │   ├── Estudiantes.Domain/
    │   ├── Estudiantes.Application/
    │   └── Estudiantes.Infrastructure/
    ├── Programas/           # Módulo: programa de créditos + adhesión
    ├── Catalogo/            # Módulo: materias + profesores
    └── Inscripciones/       # Módulo: inscripciones + reglas de negocio
```

## Capas hexagonales (cada módulo)

| Capa | Rol hexagonal |
|------|----------------|
| **Domain** | Núcleo — entidades, domain services, **ports** (interfaces) |
| **Application** | Orquestación — commands/queries, DTOs |
| **Infrastructure** | **Adaptadores de salida** — repositorios ADO.NET |
| **RegistroMs.Api** | **Adaptador de entrada** — controllers REST + adaptadores in-process entre módulos |

## Comunicación entre módulos

Inscripciones necesita datos de Programas, Catálogo y Estudiantes.  
En lugar de HTTP interno, usa **adaptadores in-process** (`ProgramasModuleAdapter`, etc.) que implementan los ports `IProgramasClient`, `ICatalogoClient`, `IEstudiantesClient`.

## Base de datos

**Una sola base de datos**: `registro_db`

| Tabla | Módulo |
|-------|--------|
| `estudiantes` | Estudiantes |
| `programas_creditos`, `estudiante_programa` | Programas |
| `profesores`, `materias` | Catálogo |
| `inscripciones` | Inscripciones |

Script: `database/mysql/registro_db.sql`

## Ejecutar

```bash
dotnet run --project registro-ms/RegistroMs.Api
```

Puerto: **5001**
