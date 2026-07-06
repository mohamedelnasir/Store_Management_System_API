namespace StoreManagementSystem.Middleware;

/// <summary>Thrown when a client sends invalid data (maps to HTTP 400).</summary>
public class ValidationAppException : Exception
{
    public ValidationAppException(string message) : base(message) { }
}

/// <summary>Thrown when a requested resource does not exist (maps to HTTP 404).</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>Thrown for auth failures such as bad credentials or duplicate email (maps to HTTP 401/409).</summary>
public class UnauthorizedAppException : Exception
{
    public UnauthorizedAppException(string message) : base(message) { }
}

/// <summary>Thrown when an operation conflicts with existing data, e.g. duplicate SKU (maps to HTTP 409).</summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
