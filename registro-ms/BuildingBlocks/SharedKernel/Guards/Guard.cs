namespace SharedKernel.Guards;

public static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be null or empty.", parameterName);
        return value;
    }

    public static T AgainstNull<T>(T? value, string parameterName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);
        return value;
    }
}
