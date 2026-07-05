namespace Blazor.Shared.Options;

public class ApiGatewayOptions
{
    public const string SectionName = "ApiGateway";
    public string BaseUrl { get; set; } = "http://localhost:5050";
}
