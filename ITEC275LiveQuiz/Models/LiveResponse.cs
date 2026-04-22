namespace ITEC275LiveQuiz.Models;

public class LiveResponse
{
    public int LiveResponseId { get; set; }
    public int LiveQuestionId { get; set; }
    public int LiveParticipantId { get; set; }
    public int AnswerId { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    public bool IsCorrect { get; set; }
    public int TimeMs { get; set; }

    public LiveQuestion? LiveQuestion { get; set; }
    public LiveParticipant? LiveParticipant { get; set; }
    public Answer? Answer { get; set; }
}
