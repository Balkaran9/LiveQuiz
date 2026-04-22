using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Host;

public class LobbyModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public LiveGame? Game { get; set; }
    public List<LiveParticipant> Participants { get; set; } = [];
    public int TotalQuestions { get; set; }
    public int OpenedQuestions { get; set; }
    public int? OpenLiveQuestionId { get; set; }

    public async Task<IActionResult> OnGetAsync(int gameId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        try
        {
            Game = await dbContext.LiveGames
                .AsNoTracking()
                .Include(g => g.Quiz)
                .FirstOrDefaultAsync(g => g.LiveGameId == gameId && g.HostUserId == userId.Value);

            if (Game is null)
            {
                return RedirectToPage("Dashboard");
            }

            Participants = await dbContext.LiveParticipants
                .AsNoTracking()
                .Where(p => p.LiveGameId == gameId)
                .OrderBy(p => p.JoinedAt)
                .ToListAsync();

            var questionStats = await dbContext.Questions
                .Where(q => q.QuizId == Game.QuizId)
                .GroupJoin(
                    dbContext.LiveQuestions.Where(lq => lq.LiveGameId == gameId),
                    q => q.QuestionId,
                    lq => lq.QuestionId,
                    (q, lq) => new { IsOpened = lq.Any() })
                .AsNoTracking()
                .ToListAsync();

            TotalQuestions = questionStats.Count;
            OpenedQuestions = questionStats.Count(s => s.IsOpened);
            
            OpenLiveQuestionId = await dbContext.LiveQuestions
                .AsNoTracking()
                .Where(q => q.LiveGameId == gameId && q.ClosedAt == null)
                .Select(q => (int?)q.LiveQuestionId)
                .FirstOrDefaultAsync();

            return Page();
        }
        catch (Exception)
        {
            return RedirectToPage("Dashboard");
        }
    }
}
