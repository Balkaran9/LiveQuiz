using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using ITEC275LiveQuiz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Host;

public class LeaderboardModel(AppDbContext dbContext, LeaderboardService leaderboardService) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public LiveGame? Game { get; set; }
    public List<LeaderboardEntry> Entries { get; set; } = [];

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

            Entries = await leaderboardService.GetLeaderboardAsync(gameId);
            return Page();
        }
        catch (Exception)
        {
            return RedirectToPage("Dashboard");
        }
    }
}
