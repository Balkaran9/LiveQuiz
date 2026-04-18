using System.ComponentModel.DataAnnotations;

namespace ITEC275LiveQuiz.Models;

public class LiveGame
{
    public int LiveGameId { get; set; }
    public int QuizId { get; set; }
    public int HostUserId { get; set; }

    [Required]
    [StringLength(6)]
    public string JoinCode { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Lobby";

    public Quiz? Quiz { get; set; }
    public User? HostUser { get; set; }
    public ICollection<LiveParticipant> Participants { get; set; } = new List<LiveParticipant>();
    public ICollection<LiveQuestion> LiveQuestions { get; set; } = new List<LiveQuestion>();
}
