# Gateway — API Gateway (YARP)

**Separado de `registro-ms`** — no usa arquitectura hexagonal.

Su única responsabilidad es **enrutar** peticiones del frontend hacia el microservicio `registro-ms`.

```
Cliente → gateway/:5000 → registro-ms/:5001
```

## Ejecutar

```bash
dotnet run --project gateway/ApiGateway
```

Puerto: **5000**
