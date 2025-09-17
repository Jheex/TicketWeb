// Arquivo: PIM/Data/AppDbContext.cs

using Microsoft.EntityFrameworkCore;
using PIM.Models;

namespace PIM.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Chamado> Chamados { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração do relacionamento Chamado -> Admin (AtribuidoA)
            modelBuilder.Entity<Chamado>()
                .HasOne(c => c.AtribuidoA)
                .WithMany()
                .HasForeignKey(c => c.AtribuidoA_AdminId)
                .OnDelete(DeleteBehavior.SetNull);

        }
    }
}