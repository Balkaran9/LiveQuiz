using System.ComponentModel.DataAnnotations;

namespace ITEC275LiveQuiz.Models;

public class LiveParticipant
{
    public int LiveParticipantId { get; set; }
    public int LiveGameId { get; set; }

    [Required]
    [StringLength(50)]
    public string Nickname { get; set; } = string.Empty;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public LiveGame? LiveGame { get; set; }
    public ICollection<LiveResponse> Responses { get; set; } = new List<LiveResponse>();
}
