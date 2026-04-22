using System.ComponentModel.DataAnnotations;

namespace ITEC275LiveQuiz.Models;

public class Quiz
{
    public int QuizId { get; set; }
    public int OwnerUserId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? OwnerUser { get; set; }
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<LiveGame> LiveGames { get; set; } = new List<LiveGame>();
}
