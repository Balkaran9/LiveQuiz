using System.ComponentModel.DataAnnotations;

namespace ITEC275LiveQuiz.Models;

public class User
{
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [StringLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Quiz> OwnedQuizzes { get; set; } = new List<Quiz>();
    public ICollection<LiveGame> HostedGames { get; set; } = new List<LiveGame>();
}
