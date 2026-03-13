using System.ComponentModel.DataAnnotations;

namespace TrainingCentreManagementSystem.Models
{
    public class UserLogin
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public string Role { get; set; }
    }
}