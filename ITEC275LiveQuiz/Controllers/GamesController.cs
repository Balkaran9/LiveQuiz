using Microsoft.AspNetCore.Mvc;
using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Services;
using ITEC275LiveQuiz.Models;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController(
    AppDbContext dbContext,
    LeaderboardService leaderboardService,
    ILogger<GamesController> logger) : ControllerBase
{
    [HttpGet("{gameId}/leaderboard")]
    public async Task<ActionResult<ApiResponse<LeaderboardResponse>>> GetLeaderboard(int gameId)
    {
        try
        {
            var game = await dbContext.LiveGames
                .Include(g => g.Quiz)
                .FirstOrDefaultAsync(g => g.LiveGameId == gameId);

            if (game is null)
            {
                return NotFound(new ApiResponse<LeaderboardResponse>
                {
                    Success = false,
                    Message = "Game not found"
                });
            }

            var entries = await leaderboardService.GetLeaderboardAsync(gameId);

            var response = new LeaderboardResponse
            {
                GameId = gameId,
                QuizTitle = game.Quiz?.Title ?? "Unknown",
                GameStatus = game.Status.ToString(),
                ParticipantCount = entries.Count,
                Leaderboard = entries.Select((e, index) => new LeaderboardEntryDto
                {
                    Rank = index + 1,
                    Nickname = e.Nickname,
                    Score = e.Score,
                    Correct = e.TotalCorrect,
                    Incorrect = e.TotalIncorrect,
                    AverageTimeSeconds = Math.Round(e.AvgTimeMs / 1000.0, 2)
                }).ToList()
            };

            return Ok(new ApiResponse<LeaderboardResponse>
            {
                Success = true,
                Data = response,
                Message = $"Leaderboard for game {gameId}"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching leaderboard for game {GameId}", gameId);
            return StatusCode(500, new ApiResponse<LeaderboardResponse>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("{gameId}/status")]
    public async Task<ActionResult<ApiResponse<GameStatusDto>>> GetGameStatus(int gameId)
    {
        try
        {
            var game = await dbContext.LiveGames
                .Include(g => g.Quiz)
                    .ThenInclude(q => q!.Questions)
                .Include(g => g.Participants)
                .FirstOrDefaultAsync(g => g.LiveGameId == gameId);

            if (game is null)
            {
                return NotFound(new ApiResponse<GameStatusDto>
                {
                    Success = false,
                    Message = "Game not found"
                });
            }

            var liveQuestions = await dbContext.LiveQuestions
                .Where(lq => lq.LiveGameId == gameId)
                .ToListAsync();

            var dto = new GameStatusDto
            {
                GameId = gameId,
                JoinCode = game.JoinCode,
                Status = game.Status.ToString(),
                QuizTitle = game.Quiz?.Title ?? "Unknown",
                ParticipantCount = game.Participants?.Count ?? 0,
                QuestionCount = game.Quiz?.Questions?.Count ?? 0,
                CurrentQuestionNumber = liveQuestions.Count(q => q.ClosedAt != null),
                StartedAt = game.StartedAt,
                EndedAt = game.EndedAt
            };

            return Ok(new ApiResponse<GameStatusDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching status for game {GameId}", gameId);
            return StatusCode(500, new ApiResponse<GameStatusDto>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpPost("join")]
    public async Task<ActionResult<ApiResponse<JoinGameResponse>>> JoinGame([FromBody] JoinGameRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.JoinCode) || string.IsNullOrWhiteSpace(request.Nickname))
            {
                return BadRequest(new ApiResponse<JoinGameResponse>
                {
                    Success = false,
                    Message = "Join code and nickname are required"
                });
            }

            var game = await dbContext.LiveGames
                .Include(g => g.Participants)
                .FirstOrDefaultAsync(g => g.JoinCode == request.JoinCode.ToUpper());

            if (game is null)
            {
                return NotFound(new ApiResponse<JoinGameResponse>
                {
                    Success = false,
                    Message = "Invalid join code"
                });
            }

            if (game.Status != "Lobby")
            {
                return BadRequest(new ApiResponse<JoinGameResponse>
                {
                    Success = false,
                    Message = "Game is not accepting new participants"
                });
            }

            var existingParticipant = game.Participants?.FirstOrDefault(p => 
                p.Nickname.Equals(request.Nickname, StringComparison.OrdinalIgnoreCase));

            if (existingParticipant is not null)
            {
                return BadRequest(new ApiResponse<JoinGameResponse>
                {
                    Success = false,
                    Message = "Nickname already taken in this game"
                });
            }

            var participant = new Models.LiveParticipant
            {
                LiveGameId = game.LiveGameId,
                Nickname = request.Nickname,
                JoinedAt = DateTime.UtcNow
            };

            dbContext.LiveParticipants.Add(participant);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Participant {Nickname} joined game {GameId} via API", 
                request.Nickname, game.LiveGameId);

            return Ok(new ApiResponse<JoinGameResponse>
            {
                Success = true,
                Data = new JoinGameResponse
                {
                    GameId = game.LiveGameId,
                    ParticipantId = participant.LiveParticipantId,
                    Nickname = participant.Nickname,
                    Message = $"Successfully joined game!"
                },
                Message = "Joined successfully"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error joining game with code {JoinCode}", request.JoinCode);
            return StatusCode(500, new ApiResponse<JoinGameResponse>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }
}

public class LeaderboardResponse
{
    public int GameId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public string GameStatus { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
    public List<LeaderboardEntryDto> Leaderboard { get; set; } = [];
}

public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public int Score { get; set; }
    public int Correct { get; set; }
    public int Incorrect { get; set; }
    public double AverageTimeSeconds { get; set; }
}

public class GameStatusDto
{
    public int GameId { get; set; }
    public string JoinCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string QuizTitle { get; set; } = string.Empty;
    public int ParticipantCount { get; set; }
    public int QuestionCount { get; set; }
    public int CurrentQuestionNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
}

public class JoinGameRequest
{
    public string JoinCode { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
}

public class JoinGameResponse
{
    public int GameId { get; set; }
    public int ParticipantId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
