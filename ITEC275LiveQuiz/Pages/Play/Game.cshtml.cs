using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Play;

[IgnoreAntiforgeryToken]
public class GameModel(AppDbContext dbContext) : ITEC275LiveQuiz.Pages.AppPageModel
{
    public LiveGame? Game { get; set; }
    public LiveParticipant? Participant { get; set; }
    public LiveQuestion? CurrentQuestion { get; set; }
    public Question? Question { get; set; }
    public List<Answer> Answers { get; set; } = [];
    public bool AlreadyAnswered { get; set; }
    public bool GameEnded { get; set; }
    public LiveResponse? LastResponse { get; set; }

    [BindProperty]
    public int SelectedAnswerId { get; set; }

    [BindProperty]
    public int ElapsedMs { get; set; }

    [BindProperty]
    public int GameId { get; set; }

    [BindProperty]
    public int ParticipantId { get; set; }

    [BindProperty]
    public int LiveQuestionId { get; set; }

    public async Task<IActionResult> OnGetAsync(int gameId)
    {
        System.Diagnostics.Debug.WriteLine($"GET: Loading game {gameId}");
        
        Game = await dbContext.LiveGames
            .AsNoTracking()
            .Include(g => g.Quiz)
            .FirstOrDefaultAsync(g => g.LiveGameId == gameId);

        if (Game is null)
        {
            System.Diagnostics.Debug.WriteLine("GET: Game not found");
            return NotFound();
        }

        var participantId = GetParticipantId(gameId);
        if (!participantId.HasValue)
        {
            System.Diagnostics.Debug.WriteLine("GET: No participant ID in session");
            return RedirectToPage("Join", new { code = Game.JoinCode });
        }

        System.Diagnostics.Debug.WriteLine($"GET: Participant ID = {participantId}");

        Participant = await dbContext.LiveParticipants
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.LiveParticipantId == participantId.Value && p.LiveGameId == gameId);

        if (Participant is null)
        {
            System.Diagnostics.Debug.WriteLine("GET: Participant not found in database");
            return RedirectToPage("Join");
        }

        GameEnded = Game.Status == "Ended";
        GameId = gameId;

        if (GameEnded)
        {
            System.Diagnostics.Debug.WriteLine("GET: Game has ended, redirecting to Stats");
            return RedirectToPage("Stats", new { gameId = gameId });
        }

        if (!GameEnded)
        {
            CurrentQuestion = await dbContext.LiveQuestions
                .AsNoTracking()
                .Include(lq => lq.Question)
                .FirstOrDefaultAsync(lq => lq.LiveGameId == gameId && lq.ClosedAt == null);

            System.Diagnostics.Debug.WriteLine($"GET: CurrentQuestion = {(CurrentQuestion == null ? "NULL" : CurrentQuestion.LiveQuestionId.ToString())}");

            if (CurrentQuestion is not null)
            {
                Question = CurrentQuestion.Question;
                Answers = await dbContext.Answers
                    .AsNoTracking()
                    .Where(a => a.QuestionId == CurrentQuestion.QuestionId)
                    .OrderBy(a => a.AnswerId)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"GET: Loaded {Answers.Count} answers");

                AlreadyAnswered = await dbContext.LiveResponses
                    .AsNoTracking()
                    .AnyAsync(r => r.LiveQuestionId == CurrentQuestion.LiveQuestionId
                               && r.LiveParticipantId == Participant.LiveParticipantId);

                System.Diagnostics.Debug.WriteLine($"GET: AlreadyAnswered = {AlreadyAnswered}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("GET: No current question found");
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int gameId)
    {
        GameId = gameId;

        try
        {
            // Use ParticipantId sent directly from the page (no session needed)
            int participantId = ParticipantId;

            // Fallback to session/cookie if not sent
            if (participantId <= 0)
            {
                var fromSession = GetParticipantId(GameId);
                if (!fromSession.HasValue)
                {
                    return RedirectToPage("Join");
                }
                participantId = fromSession.Value;
            }

            System.Diagnostics.Debug.WriteLine($"POST: ParticipantId={participantId}, AnswerId={SelectedAnswerId}, GameId={GameId}");

            var game = await dbContext.LiveGames
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.LiveGameId == GameId);

            if (game is null || game.Status == "Ended")
                return RedirectToPage("Stats", new { gameId = GameId });

            // Use the LiveQuestionId sent from page if available, else look it up
            LiveQuestion? liveQuestion = null;
            if (LiveQuestionId > 0)
            {
                liveQuestion = await dbContext.LiveQuestions
                    .AsNoTracking()
                    .Include(lq => lq.Question)
                    .FirstOrDefaultAsync(lq => lq.LiveQuestionId == LiveQuestionId && lq.LiveGameId == GameId);
            }

            if (liveQuestion is null)
            {
                liveQuestion = await dbContext.LiveQuestions
                    .AsNoTracking()
                    .Include(lq => lq.Question)
                    .FirstOrDefaultAsync(lq => lq.LiveGameId == GameId && lq.ClosedAt == null);
            }

            if (liveQuestion is null)
                return RedirectToPage("Game", new { gameId = GameId });

            var alreadyAnswered = await dbContext.LiveResponses
                .AsNoTracking()
                .AnyAsync(r => r.LiveQuestionId == liveQuestion.LiveQuestionId
                           && r.LiveParticipantId == participantId);

            System.Diagnostics.Debug.WriteLine($"POST: AlreadyAnswered={alreadyAnswered}, SelectedId={SelectedAnswerId}");

            if (!alreadyAnswered && SelectedAnswerId > 0)
            {
                var answer = await dbContext.Answers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AnswerId == SelectedAnswerId
                                           && a.QuestionId == liveQuestion.QuestionId);

                if (answer is not null)
                {
                    dbContext.LiveResponses.Add(new LiveResponse
                    {
                        LiveQuestionId = liveQuestion.LiveQuestionId,
                        LiveParticipantId = participantId,
                        AnswerId = answer.AnswerId,
                        AnsweredAt = DateTime.UtcNow,
                        IsCorrect = answer.IsCorrect,
                        TimeMs = Math.Max(0, ElapsedMs)
                    });
                    await dbContext.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine($"POST: Answer saved! Correct={answer.IsCorrect}");
                }
            }

            return RedirectToPage("Game", new { gameId = GameId });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"POST ERROR: {ex.Message}");
            return RedirectToPage("Game", new { gameId = GameId });
        }
    }
}
