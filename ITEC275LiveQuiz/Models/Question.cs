using System.ComponentModel.DataAnnotations;

namespace ITEC275LiveQuiz.Models;

public class Question
{
    public int QuestionId { get; set; }
    public int QuizId { get; set; }

    [Required]
    [StringLength(500)]
    public string QuestionText { get; set; } = string.Empty;

    [Range(5, 300)]
    public int TimeLimitSeconds { get; set; } = 30;

    public int SortOrder { get; set; }

    public Quiz? Quiz { get; set; }
    public ICollection<Answer> Answers { get; set; } = new List<Answer>();
    public ICollection<LiveQuestion> LiveQuestions { get; set; } = new List<LiveQuestion>();
}
