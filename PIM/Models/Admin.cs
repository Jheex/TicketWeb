using System;
using System.ComponentModel.DataAnnotations;

namespace PIM.Models
{
    public class Admin
    {
        public int Id { get; set; }

        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
