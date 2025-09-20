// Arquivo: PIM/Data/AppDbContext.cs

using Microsoft.EntityFrameworkCore;
using PIM.Models;

namespace PIM.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tabelas do banco
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Chamado> Chamados { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração do relacionamento Chamado -> Usuario (AtribuidoA)
            modelBuilder.Entity<Chamado>()
                .HasOne(c => c.AtribuidoA)   // Navegação para Usuario
                .WithMany()                  // Usuário não precisa de coleção de Chamados
                .HasForeignKey(c => c.AtribuidoAId)  // Aqui é a FK que é int?
                .OnDelete(DeleteBehavior.SetNull);

            // Configuração de propriedades opcionais/nulas, se necessário
            modelBuilder.Entity<Chamado>()
                .Property(c => c.DataFechamento)
                .IsRequired(false);

            modelBuilder.Entity<Chamado>()
                .Property(c => c.DataAtribuicao)
                .IsRequired(false);

            modelBuilder.Entity<Chamado>()
                .Property(c => c.NomeArquivoAnexo)
                .HasMaxLength(255)
                .IsRequired(false);

            modelBuilder.Entity<Chamado>()
                .Property(c => c.CaminhoArquivoAnexo)
                .HasMaxLength(255)
                .IsRequired(false);

            modelBuilder.Entity<Usuario>()
                .Property(u => u.Username)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Usuario>()
                .Property(u => u.Email)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Usuario>()
                .Property(u => u.SenhaHash)
                .HasMaxLength(255)
                .IsRequired();
        }
    }
}
