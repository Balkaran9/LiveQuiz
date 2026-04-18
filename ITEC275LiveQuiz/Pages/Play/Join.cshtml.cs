using System.ComponentModel.DataAnnotations;
using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ITEC275LiveQuiz.Hubs;

namespace ITEC275LiveQuiz.Pages.Play;

public class JoinModel(
    AppDbContext dbContext, 
    IHubContext<GameHub> hubContext,
    ILogger<JoinModel> logger) : ITEC275LiveQuiz.Pages.AppPageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet(string? code)
    {
        if (!string.IsNullOrWhiteSpace(code))
        {
            Input.JoinCode = code.ToUpperInvariant();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var code = Input.JoinCode.Trim().ToUpperInvariant();
        var game = await dbContext.LiveGames
            .FirstOrDefaultAsync(g => g.JoinCode == code && (g.Status == "Lobby" || g.Status == "InProgress"));

        if (game is null)
        {
            ModelState.AddModelError(string.Empty, "No active game found with that join code.");
            return Page();
        }

        var nickname = Input.Nickname.Trim();
        var nickTaken = await dbContext.LiveParticipants
            .AnyAsync(p => p.LiveGameId == game.LiveGameId && p.Nickname == nickname);

        if (nickTaken)
        {
            ModelState.AddModelError(string.Empty, "That nickname is already taken in this game.");
            return Page();
        }

        var participant = new LiveParticipant
        {
            LiveGameId = game.LiveGameId,
            Nickname = nickname,
            JoinedAt = DateTime.UtcNow
        };

        dbContext.LiveParticipants.Add(participant);
        await dbContext.SaveChangesAsync();

        // Notify host via SignalR
        try
        {
            await hubContext.Clients.Group($"game_{game.LiveGameId}")
                .SendAsync("ParticipantJoined", nickname);
            logger.LogInformation("SignalR notification sent for {Nickname} joining game {GameId}", 
                nickname, game.LiveGameId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send SignalR notification");
        }

        SetParticipantId(game.LiveGameId, participant.LiveParticipantId);

        logger.LogInformation("Participant {Nickname} joined game {GameId}", nickname, game.LiveGameId);

        return RedirectToPage("Game", new { gameId = game.LiveGameId });
    }

    public class InputModel
    {
        [Required]
        [StringLength(6, MinimumLength = 6)]
        [Display(Name = "Join Code")]
        public string JoinCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Nickname")]
        public string Nickname { get; set; } = string.Empty;
    }
}
