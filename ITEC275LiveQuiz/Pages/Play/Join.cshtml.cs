using System.ComponentModel.DataAnnotations;
using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Play;

public class JoinModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
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

        var code = Input.JoinCode?.Trim().ToUpperInvariant() ?? string.Empty;
        if (string.IsNullOrEmpty(code))
        {
            ModelState.AddModelError(string.Empty, "Please enter a valid join code.");
            return Page();
        }

        var game = await dbContext.LiveGames
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.JoinCode == code && (g.Status == "Lobby" || g.Status == "InProgress"));

        if (game is null)
        {
            ModelState.AddModelError(string.Empty, "No active game found with that join code.");
            return Page();
        }

        var nickname = Input.Nickname?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(nickname))
        {
            ModelState.AddModelError(string.Empty, "Please enter a valid nickname.");
            return Page();
        }

        var nickTaken = await dbContext.LiveParticipants
            .AsNoTracking()
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

        SetParticipantId(game.LiveGameId, participant.LiveParticipantId);

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
