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

        Game = await dbContext.LiveGames
            .Include(g => g.Quiz)
            .FirstOrDefaultAsync(g => g.LiveGameId == gameId && g.HostUserId == userId.Value);

        if (Game is null)
        {
            return NotFound();
        }

        Participants = await dbContext.LiveParticipants
            .Where(p => p.LiveGameId == gameId)
            .OrderBy(p => p.JoinedAt)
            .ToListAsync();

        TotalQuestions = await dbContext.Questions.CountAsync(q => q.QuizId == Game.QuizId);
        OpenedQuestions = await dbContext.LiveQuestions.CountAsync(q => q.LiveGameId == gameId);
        OpenLiveQuestionId = await dbContext.LiveQuestions
            .Where(q => q.LiveGameId == gameId && q.ClosedAt == null)
            .Select(q => (int?)q.LiveQuestionId)
            .FirstOrDefaultAsync();

        return Page();
    }
}
