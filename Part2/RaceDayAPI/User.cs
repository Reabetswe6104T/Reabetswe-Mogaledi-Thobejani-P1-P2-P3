using System.ComponentModel.DataAnnotations;
namespace RaceDayAPI.Models
{
    public class User
    {
        [Key] public int UserID { get; set; }
        [Required, EmailAddress, MaxLength(100)] public string Email { get; set; } = string.Empty;
        [Required] public string PasswordHash { get; set; } = string.Empty;
        [Required, MaxLength(20)] public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }
        public Participant? Participant { get; set; }
        public Organiser? Organiser { get; set; }
    }
}
