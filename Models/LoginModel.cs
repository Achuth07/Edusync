using System.ComponentModel.DataAnnotations;

namespace Edusync.Models
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; } // Changed from Email to Username

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
