using DentalCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DentalCare.Infrastructure.Data;

public class DentalCareDbContext : DbContext
{
    public DentalCareDbContext(DbContextOptions<DentalCareDbContext> options) : base(options)
    {
    }

    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Odontologo> Odontologos => Set<Odontologo>();
    public DbSet<Consultorio> Consultorios => Set<Consultorio>();
    public DbSet<Tratamiento> Tratamientos => Set<Tratamiento>();
    public DbSet<Turno> Turnos => Set<Turno>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Paciente ----------
        modelBuilder.Entity<Paciente>(e =>
        {
            e.ToTable("paciente");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("id_paciente");
            e.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            e.Property(p => p.Apellido).HasColumnName("apellido").HasMaxLength(100).IsRequired();
            e.Property(p => p.Dni).HasColumnName("dni").HasMaxLength(20).IsRequired();
            e.Property(p => p.FechaNacimiento).HasColumnName("fecha_nacimiento");
            e.Property(p => p.Telefono).HasColumnName("telefono").HasMaxLength(30);
            e.Property(p => p.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
            e.Property(p => p.Direccion).HasColumnName("direccion").HasMaxLength(200);
            e.Property(p => p.ObraSocial).HasColumnName("obra_social").HasMaxLength(100);
            e.Property(p => p.NumeroAfiliado).HasColumnName("numero_afiliado").HasMaxLength(50);
            e.HasIndex(p => p.Dni).IsUnique();
            e.HasIndex(p => p.Email).IsUnique();
        });

        // ---------- Odontologo ----------
        modelBuilder.Entity<Odontologo>(e =>
        {
            e.ToTable("odontologo");
            e.HasKey(o => o.Id);
            e.Property(o => o.Id).HasColumnName("id_odontologo");
            e.Property(o => o.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            e.Property(o => o.Apellido).HasColumnName("apellido").HasMaxLength(100).IsRequired();
            e.Property(o => o.Dni).HasColumnName("dni").HasMaxLength(20).IsRequired();
            e.Property(o => o.Matricula).HasColumnName("matricula").HasMaxLength(50).IsRequired();
            e.Property(o => o.Especialidad).HasColumnName("especialidad").HasMaxLength(50).IsRequired();
            e.Property(o => o.Telefono).HasColumnName("telefono").HasMaxLength(30);
            e.Property(o => o.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
            e.HasIndex(o => o.Dni).IsUnique();
            e.HasIndex(o => o.Matricula).IsUnique();
            e.HasIndex(o => o.Email).IsUnique();
        });

        // ---------- Consultorio ----------
        modelBuilder.Entity<Consultorio>(e =>
        {
            e.ToTable("consultorio");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("id_consultorio");
            e.Property(c => c.Numero).HasColumnName("numero");
            e.Property(c => c.Piso).HasColumnName("piso");
            e.Property(c => c.Nombre).HasColumnName("nombre").HasMaxLength(100);
            e.Property(c => c.Equipamiento).HasColumnName("equipamiento").HasColumnType("text");
        });

        // ---------- Tratamiento ----------
        modelBuilder.Entity<Tratamiento>(e =>
        {
            e.ToTable("tratamiento");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasColumnName("id_tratamiento");
            e.Property(t => t.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
            e.Property(t => t.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            e.Property(t => t.Descripcion).HasColumnName("descripcion").HasColumnType("text");
            e.Property(t => t.Costo).HasColumnName("costo").HasPrecision(10, 2);
            e.Property(t => t.DuracionEstimada).HasColumnName("duracion_estimada");
            e.HasIndex(t => t.Codigo).IsUnique();
        });

        // ---------- Turno ----------
        modelBuilder.Entity<Turno>(e =>
        {
            e.ToTable("turno");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasColumnName("id_turno");
            e.Property(t => t.Fecha).HasColumnName("fecha");
            e.Property(t => t.Hora).HasColumnName("hora");
            e.Property(t => t.Duracion).HasColumnName("duracion");
            e.Property(t => t.Estado).HasColumnName("estado").HasMaxLength(30).IsRequired();
            e.Property(t => t.Motivo).HasColumnName("motivo").HasMaxLength(255);
            e.Property(t => t.Observaciones).HasColumnName("observaciones").HasColumnType("text");
            e.Property(t => t.PacienteId).HasColumnName("id_paciente");
            e.Property(t => t.OdontologoId).HasColumnName("id_odontologo");
            e.Property(t => t.ConsultorioId).HasColumnName("id_consultorio");

            // Paciente (1) --solicita--> Turno (0..*)
            e.HasOne(t => t.Paciente)
                .WithMany(p => p.Turnos)
                .HasForeignKey(t => t.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Odontologo (1) --atiende--> Turno (0..*)
            e.HasOne(t => t.Odontologo)
                .WithMany(o => o.Turnos)
                .HasForeignKey(t => t.OdontologoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Consultorio (1) --se realiza en--> Turno (0..*)
            e.HasOne(t => t.Consultorio)
                .WithMany(c => c.Turnos)
                .HasForeignKey(t => t.ConsultorioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Turno (0..*) --incluye--> Tratamiento (1..*): N:N.
            // El diagrama no tiene entidad intermedia, por eso la tabla "turno_tratamiento"
            // la crea EF Core solo, con las FK id_turno e id_tratamiento (PK compuesta).
            e.HasMany(t => t.Tratamientos)
                .WithMany(x => x.Turnos)
                .UsingEntity<Dictionary<string, object>>(
                    "turno_tratamiento",
                    right => right.HasOne<Tratamiento>().WithMany().HasForeignKey("id_tratamiento"),
                    left => left.HasOne<Turno>().WithMany().HasForeignKey("id_turno"));

            // Índices de apoyo para el futuro control de superposición de horarios.
            e.HasIndex(t => new { t.OdontologoId, t.Fecha, t.Hora });
            e.HasIndex(t => new { t.ConsultorioId, t.Fecha, t.Hora });
        });
    }
}
