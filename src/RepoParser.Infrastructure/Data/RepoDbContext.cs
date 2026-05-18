using Microsoft.EntityFrameworkCore;
using RepoParser.Core.Entities;

namespace RepoParser.Infrastructure.Data;

public class RepoDbContext : DbContext
{
    public RepoDbContext(DbContextOptions<RepoDbContext> options) : base(options) { }

    public DbSet<SourceFile> SourceFiles => Set<SourceFile>();
    public DbSet<MethodDefinition> MethodDefinitions => Set<MethodDefinition>();
    public DbSet<DriftAlert> DriftAlerts => Set<DriftAlert>();
    public DbSet<DependencyEdge> DependencyEdges => Set<DependencyEdge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SourceFile>(entity =>
        {
            entity.HasIndex(e => e.FilePath).IsUnique();
            entity.Property(e => e.FilePath).HasMaxLength(1024);
            entity.Property(e => e.Language).HasMaxLength(50);
            entity.Property(e => e.ContentHash).HasMaxLength(64);
        });

        modelBuilder.Entity<MethodDefinition>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.Signature).HasMaxLength(2048);
            entity.Property(e => e.ReturnType).HasMaxLength(256);
            entity.Property(e => e.Parameters).HasColumnType("TEXT");
            entity.Property(e => e.DocComment).HasColumnType("TEXT");
            entity.Property(e => e.CodeEmbedding).HasColumnType("BLOB");
            entity.Property(e => e.DocEmbedding).HasColumnType("BLOB");
            entity.HasOne(e => e.SourceFile)
                  .WithMany(s => s.Methods)
                  .HasForeignKey(e => e.SourceFileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DriftAlert>(entity =>
        {
            entity.Property(e => e.Message).HasMaxLength(2048);
            entity.Property(e => e.OldSignature).HasMaxLength(2048);
            entity.Property(e => e.NewSignature).HasMaxLength(2048);
            entity.HasOne(e => e.MethodDefinition)
                  .WithMany(m => m.Alerts)
                  .HasForeignKey(e => e.MethodDefinitionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DependencyEdge>(entity =>
        {
            entity.Property(e => e.TargetFilePath).HasMaxLength(1024);
            entity.Property(e => e.DependencyType).HasMaxLength(50);
            entity.HasOne(e => e.SourceFile)
                  .WithMany()
                  .HasForeignKey(e => e.SourceFileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
