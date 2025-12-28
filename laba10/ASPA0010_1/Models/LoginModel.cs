using System.ComponentModel.DataAnnotations;

namespace ASPA0010_1.Models
{
    public class LoginModel { 
        [Required] 
        public string Login { get; set; } 
        [Required] 
        public string Password { get; set; } 
    }
}
