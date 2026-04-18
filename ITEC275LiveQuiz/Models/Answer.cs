using System.ComponentModel.DataAnnotations;

namespace ITEC275LiveQuiz.Models;

public class Answer
{
    public int AnswerId { get; set; }
    public int QuestionId { get; set; }

    [Required]
    [StringLength(250)]
    public string AnswerText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public Question? Question { get; set; }
    public ICollection<LiveResponse> LiveResponses { get; set; } = new List<LiveResponse>();
}
