namespace DentalCare.Domain.Exceptions;

/// <summary>Violación de una regla de negocio (se traduce a HTTP 409 / 400).</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}

/// <summary>La entidad buscada no existe (se traduce a HTTP 404).</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entidad, object id)
        : base($"{entidad} con id {id} no encontrado.")
    {
    }
}

/// <summary>El usuario autenticado no tiene permiso sobre el recurso (se traduce a HTTP 403).</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}

/// <summary>Credenciales inválidas (se traduce a HTTP 401).</summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
