using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using ITEC275LiveQuiz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Host;

public class StartModel(AppDbContext dbContext, JoinCodeService joinCodeService) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public async Task<IActionResult> OnGetAsync(int quizId)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return RedirectToLogin();
        }

        var quiz = await dbContext.Quizzes.FirstOrDefaultAsync(q => q.QuizId == quizId && q.OwnerUserId == userId.Value);
        if (quiz is null)
        {
            return NotFound();
        }

        var liveGame = new LiveGame
        {
            QuizId = quizId,
            HostUserId = userId.Value,
            JoinCode = await joinCodeService.GenerateUniqueCodeAsync(),
            StartedAt = DateTime.UtcNow,
            Status = "Lobby"
        };

        dbContext.LiveGames.Add(liveGame);
        await dbContext.SaveChangesAsync();

        return RedirectToPage("Lobby", new { gameId = liveGame.LiveGameId });
    }
}
