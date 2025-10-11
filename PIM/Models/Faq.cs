using System;

namespace PIM.Models
{
    public class Faq
    {
        public int Id { get; set; }
        public string ?Pergunta { get; set; }
        public string ?Resposta { get; set; }
        public string ?Categoria { get; set; }
        public int Ordem { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime DataAtualizacao { get; set; } = DateTime.Now;
    }
}
