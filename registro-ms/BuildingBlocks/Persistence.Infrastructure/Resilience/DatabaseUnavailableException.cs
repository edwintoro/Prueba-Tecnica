using System.Data.Common;
using Persistence.Abstractions;

namespace Persistence.Infrastructure.Resilience;

public sealed class DatabaseUnavailableException : Exception
{
    public DatabaseUnavailableException(string message) : base(message) { }
    public DatabaseUnavailableException(string message, Exception inner) : base(message, inner) { }
}
