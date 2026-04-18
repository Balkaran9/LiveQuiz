using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Play;

public class StatsModel(AppDbContext dbContext) : PageModel
{
    public LiveGame? Game { get; set; }
    public LiveParticipant? Participant { get; set; }
    public List<QuestionStat> QuestionStats { get; set; } = [];
    public int TotalCorrect { get; set; }
    public int TotalQuestions { get; set; }
    public double AverageTime { get; set; }
    public int Rank { get; set; }
    public int TotalParticipants { get; set; }

    public async Task<IActionResult> OnGetAsync(int gameId, int participantId)
    {
        Game = await dbContext.LiveGames
            .Include(g => g.Quiz)
            .FirstOrDefaultAsync(g => g.LiveGameId == gameId);

        if (Game is null) return NotFound();

        Participant = await dbContext.LiveParticipants
            .FirstOrDefaultAsync(p => p.LiveParticipantId == participantId && p.LiveGameId == gameId);

        if (Participant is null) return NotFound();

        QuestionStats = await dbContext.LiveResponses
            .Where(r => r.LiveParticipantId == participantId)
            .Include(r => r.LiveQuestion)
            .ThenInclude(lq => lq!.Question)
            .Include(r => r.Answer)
            .OrderBy(r => r.LiveQuestion!.OpenedAt)
            .Select(r => new QuestionStat
            {
                QuestionText = r.LiveQuestion!.Question!.QuestionText,
                AnswerText = r.Answer!.AnswerText,
                IsCorrect = r.IsCorrect,
                TimeMs = r.TimeMs,
                Points = r.PointsEarned
            })
            .ToListAsync();

        TotalCorrect = QuestionStats.Count(q => q.IsCorrect);
        TotalQuestions = QuestionStats.Count;
        AverageTime = QuestionStats.Any() ? QuestionStats.Average(q => q.TimeMs) / 1000.0 : 0;

        var allParticipants = await dbContext.LiveParticipants
            .Where(p => p.LiveGameId == gameId)
            .Select(p => new
            {
                p.LiveParticipantId,
                Score = p.Responses.Sum(r => r.PointsEarned),
                TotalTime = p.Responses.Sum(r => r.TimeMs)
            })
            .ToListAsync();

        TotalParticipants = allParticipants.Count;
        var myScore = allParticipants.FirstOrDefault(p => p.LiveParticipantId == participantId);
        if (myScore != null)
        {
            Rank = allParticipants
                .OrderByDescending(p => p.Score)
                .ThenBy(p => p.TotalTime)
                .ToList()
                .FindIndex(p => p.LiveParticipantId == participantId) + 1;
        }

        return Page();
    }

    public class QuestionStat
    {
        public string QuestionText { get; set; } = string.Empty;
        public string AnswerText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int TimeMs { get; set; }
        public int Points { get; set; }
    }
}
