namespace DentalCare.Domain.Enums;

/// <summary>Estados posibles de un turno. Se persiste como texto en la columna "estado".</summary>
public enum EstadoTurno
{
    Pendiente,
    Confirmado,
    Ausente,
    Finalizado,
    Cancelado
}
