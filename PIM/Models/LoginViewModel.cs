using System.ComponentModel.DataAnnotations;

namespace PIM.Models
{
    public class LoginViewModel
    {
        public string Username { get; set; } = null!;
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        public bool RememberMe { get; set; } // ADICIONAR
    }
}