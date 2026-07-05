using System.Net.Http.Json;
using Blazor.Shared.Models;
using Blazor.Shared.Options;
using Microsoft.Extensions.Options;

namespace Blazor.Shared.Services;

public interface IEstudiantesApiClient
{
    Task<IReadOnlyList<EstudianteModel>> GetAllAsync();
    Task<EstudianteModel?> GetByIdAsync(int id);
    Task<EstudianteModel?> CreateAsync(CreateEstudianteModel request);
    Task<EstudianteModel?> UpdateAsync(int id, UpdateEstudianteModel request);
    Task DeleteAsync(int id);
}

public sealed class EstudiantesApiClient : IEstudiantesApiClient
{
    private readonly HttpClient _http;

    public EstudiantesApiClient(HttpClient http, IOptions<ApiGatewayOptions> options)
    {
        _http = http;
        _http.BaseAddress = new Uri(options.Value.BaseUrl);
    }

    public async Task<IReadOnlyList<EstudianteModel>> GetAllAsync() =>
        await _http.GetFromJsonAsync<List<EstudianteModel>>("api/estudiantes") ?? [];

    public async Task<EstudianteModel?> GetByIdAsync(int id) =>
        await _http.GetFromJsonAsync<EstudianteModel>($"api/estudiantes/{id}");

    public async Task<EstudianteModel?> CreateAsync(CreateEstudianteModel request)
    {
        var response = await _http.PostAsJsonAsync("api/estudiantes", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<EstudianteModel>()
            : null;
    }

    public async Task<EstudianteModel?> UpdateAsync(int id, UpdateEstudianteModel request)
    {
        var response = await _http.PutAsJsonAsync($"api/estudiantes/{id}", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<EstudianteModel>()
            : null;
    }

    public async Task DeleteAsync(int id) =>
        (await _http.DeleteAsync($"api/estudiantes/{id}")).EnsureSuccessStatusCode();
}

public interface IProgramasApiClient
{
    Task<ProgramaModel?> GetActivoAsync();
    Task<AdhesionModel?> GetAdhesionAsync(int estudianteId);
    Task<bool> AdherirAsync(AdherirRequest request);
}

public sealed class ProgramasApiClient : IProgramasApiClient
{
    private readonly HttpClient _http;

    public ProgramasApiClient(HttpClient http, IOptions<ApiGatewayOptions> options)
    {
        _http = http;
        _http.BaseAddress = new Uri(options.Value.BaseUrl);
    }

    public async Task<ProgramaModel?> GetActivoAsync() =>
        await _http.GetFromJsonAsync<ProgramaModel>("api/programas/activo");

    public async Task<AdhesionModel?> GetAdhesionAsync(int estudianteId) =>
        await _http.GetFromJsonAsync<AdhesionModel>($"api/programas/adhesion/{estudianteId}");

    public async Task<bool> AdherirAsync(AdherirRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/programas/adherir", request);
        return response.IsSuccessStatusCode;
    }
}

public interface ICatalogoApiClient
{
    Task<IReadOnlyList<MateriaModel>> GetMateriasAsync();
    Task<MateriaModel?> CreateMateriaAsync(CreateMateriaModel request);
    Task<MateriaModel?> UpdateMateriaAsync(int id, UpdateMateriaModel request);
    Task DeleteMateriaAsync(int id);
    Task<IReadOnlyList<ProfesorModel>> GetProfesoresAsync();
    Task<ProfesorModel?> CreateProfesorAsync(CreateProfesorModel request);
    Task<ProfesorModel?> UpdateProfesorAsync(int id, UpdateProfesorModel request);
    Task DeleteProfesorAsync(int id);
}

public sealed class CatalogoApiClient : ICatalogoApiClient
{
    private readonly HttpClient _http;

    public CatalogoApiClient(HttpClient http, IOptions<ApiGatewayOptions> options)
    {
        _http = http;
        _http.BaseAddress = new Uri(options.Value.BaseUrl);
    }

    public async Task<IReadOnlyList<MateriaModel>> GetMateriasAsync() =>
        await _http.GetFromJsonAsync<List<MateriaModel>>("api/materias") ?? [];

    public async Task<MateriaModel?> CreateMateriaAsync(CreateMateriaModel request)
    {
        var response = await _http.PostAsJsonAsync("api/materias", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<MateriaModel>()
            : null;
    }

    public async Task<MateriaModel?> UpdateMateriaAsync(int id, UpdateMateriaModel request)
    {
        var response = await _http.PutAsJsonAsync($"api/materias/{id}", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<MateriaModel>()
            : null;
    }

    public async Task DeleteMateriaAsync(int id) =>
        (await _http.DeleteAsync($"api/materias/{id}")).EnsureSuccessStatusCode();

    public async Task<IReadOnlyList<ProfesorModel>> GetProfesoresAsync() =>
        await _http.GetFromJsonAsync<List<ProfesorModel>>("api/profesores") ?? [];

    public async Task<ProfesorModel?> CreateProfesorAsync(CreateProfesorModel request)
    {
        var response = await _http.PostAsJsonAsync("api/profesores", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ProfesorModel>()
            : null;
    }

    public async Task<ProfesorModel?> UpdateProfesorAsync(int id, UpdateProfesorModel request)
    {
        var response = await _http.PutAsJsonAsync($"api/profesores/{id}", request);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<ProfesorModel>()
            : null;
    }

    public async Task DeleteProfesorAsync(int id) =>
        (await _http.DeleteAsync($"api/profesores/{id}")).EnsureSuccessStatusCode();
}

public interface IInscripcionesApiClient
{
    Task<IReadOnlyList<InscripcionModel>> GetByEstudianteAsync(int estudianteId);
    Task<bool> InscribirAsync(InscribirRequest request);
    Task CancelarAsync(int inscripcionId);
    Task<IReadOnlyList<string>> GetCompanerosAsync(int materiaId, int estudianteId);
}

public sealed class InscripcionesApiClient : IInscripcionesApiClient
{
    private readonly HttpClient _http;

    public InscripcionesApiClient(HttpClient http, IOptions<ApiGatewayOptions> options)
    {
        _http = http;
        _http.BaseAddress = new Uri(options.Value.BaseUrl);
    }

    public async Task<IReadOnlyList<InscripcionModel>> GetByEstudianteAsync(int estudianteId) =>
        await _http.GetFromJsonAsync<List<InscripcionModel>>($"api/inscripciones/estudiantes/{estudianteId}") ?? [];

    public async Task<bool> InscribirAsync(InscribirRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/inscripciones", request);
        return response.IsSuccessStatusCode;
    }

    public async Task CancelarAsync(int inscripcionId) =>
        (await _http.DeleteAsync($"api/inscripciones/{inscripcionId}")).EnsureSuccessStatusCode();

    public async Task<IReadOnlyList<string>> GetCompanerosAsync(int materiaId, int estudianteId) =>
        await _http.GetFromJsonAsync<List<string>>(
            $"api/inscripciones/materias/{materiaId}/companeros?estudianteId={estudianteId}") ?? [];
}
