namespace ITEC275LiveQuiz.Models;

public class LiveQuestion
{
    public int LiveQuestionId { get; set; }
    public int LiveGameId { get; set; }
    public int QuestionId { get; set; }
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public LiveGame? LiveGame { get; set; }
    public Question? Question { get; set; }
    public ICollection<LiveResponse> Responses { get; set; } = new List<LiveResponse>();
}
