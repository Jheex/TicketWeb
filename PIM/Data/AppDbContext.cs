// Arquivo: PIM/Data/AppDbContext.cs

using Microsoft.EntityFrameworkCore;
using PIM.Models;

namespace PIM.Data
{
    /// <summary>
    /// Contexto do banco de dados principal para a aplicação PIM, gerenciado pelo Entity Framework Core.
    /// <para>Define os conjuntos de dados (DbSets) e a configuração de mapeamento de modelos.</para>
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Construtor que recebe as opções de configuração do DbContext, normalmente injetadas via Startup.cs.
        /// </summary>
        /// <param name="options">As opções do DbContext.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tabelas do banco
        /// <summary>
        /// Representa a coleção de todos os usuários do sistema.
        /// </summary>
        public DbSet<Usuario> Usuarios { get; set; }
        
        /// <summary>
        /// Representa a coleção de todos os chamados (tickets) abertos.
        /// </summary>
        public DbSet<Chamado> Chamados { get; set; }
        
        /// <summary>
        /// Representa a coleção de itens de Perguntas Frequentes (FAQs).
        /// </summary>
        public DbSet<Faq> Faqs { get; set; }

        /// <summary>
        /// Configura o modelo de dados e os relacionamentos entre as entidades.
        /// </summary>
        /// <param name="modelBuilder">O construtor de modelos usado para configurar os tipos de entidade.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração do relacionamento: Chamado (FK AtribuidoAId) -> Usuario
            modelBuilder.Entity<Chamado>()
                .HasOne(c => c.AtribuidoA)   // Um chamado possui um Usuario AtribuidoA
                .WithMany()                  // O Usuario pode estar em muitos chamados (sem coleção de navegação de volta)
                .HasForeignKey(c => c.AtribuidoAId)  // Usa a chave estrangeira AtribuidoAId (int? - nullable)
                .OnDelete(DeleteBehavior.SetNull); // Se o usuário atribuído for deletado, a FK é definida como NULL.

            // Configuração de propriedades opcionais/nulas
            
            // Permite que DataFechamento seja NULL
            modelBuilder.Entity<Chamado>()
                .Property(c => c.DataFechamento)
                .IsRequired(false);

            // Permite que DataAtribuicao seja NULL
            modelBuilder.Entity<Chamado>()
                .Property(c => c.DataAtribuicao)
                .IsRequired(false);

            // Configuração de tamanho máximo e nulidade para NomeArquivoAnexo
            modelBuilder.Entity<Chamado>()
                .Property(c => c.NomeArquivoAnexo)
                .HasMaxLength(255)
                .IsRequired(false);

            // Configuração de tamanho máximo e nulidade para CaminhoArquivoAnexo
            modelBuilder.Entity<Chamado>()
                .Property(c => c.CaminhoArquivoAnexo)
                .HasMaxLength(255)
                .IsRequired(false);

            // Configuração de tamanho máximo e obrigatoriedade para Usuario
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