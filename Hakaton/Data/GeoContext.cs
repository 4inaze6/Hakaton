using Hakaton.Models;
using Microsoft.EntityFrameworkCore;

namespace Hakaton.Data;

public partial class GeoContext : DbContext
{
    public GeoContext()
    {
    }

    public GeoContext(DbContextOptions<GeoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<GeoDatum> GeoData { get; set; }
    //настройки подключения к бд
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=MSI\\MSSQLSERVER04;Database=geography;Trusted_Connection=True; TrustServerCertificate = True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GeoDatum>(entity =>
        {
            entity.HasKey(e => e.ImageId);

            entity.Property(e => e.DateTime).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
