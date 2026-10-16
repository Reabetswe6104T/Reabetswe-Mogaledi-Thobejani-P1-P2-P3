using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RaceDayAPI.Models
{
    public class Participant
    {
        [Key] public int ParticipantID { get; set; }
        [Required] public int UserID { get; set; }
        [Required, MaxLength(100)] public string FullName { get; set; } = string.Empty;
        [Required, MaxLength(13)] public string IDNumber { get; set; } = string.Empty;
        [MaxLength(15)] public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [MaxLength(10)] public string? Gender { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [ForeignKey("UserID")] public User? User { get; set; }
        public ICollection<Entry> Entries { get; set; } = new List<Entry>();
    }
}
