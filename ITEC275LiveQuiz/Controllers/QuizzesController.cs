using Microsoft.AspNetCore.Mvc;
using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using ITEC275LiveQuiz.Services;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizzesController(
    AppDbContext dbContext, 
    CacheService cacheService,
    ILogger<QuizzesController> logger) : ControllerBase
{
    [HttpGet("public")]
    public async Task<ActionResult<ApiResponse<List<QuizDto>>>> GetPublicQuizzes(
        [FromQuery] string? search,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            logger.LogInformation("API request for public quizzes (search: {Search}, category: {Category})", 
                search ?? "none", category ?? "all");

            var query = dbContext.Quizzes.Where(q => q.IsPublic);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(q => q.Title.Contains(search) || q.OwnerUser!.Username.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(q => q.Category == category);
            }

            var total = await query.CountAsync();
            var quizzes = await query
                .Include(q => q.Questions)
                .Include(q => q.OwnerUser)
                .OrderByDescending(q => q.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(q => new QuizDto
                {
                    QuizId = q.QuizId,
                    Title = q.Title,
                    Category = q.Category,
                    QuestionCount = q.Questions!.Count,
                    OwnerUsername = q.OwnerUser!.Username,
                    CreatedAt = q.CreatedAt,
                    IsPublic = q.IsPublic
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<QuizDto>>
            {
                Success = true,
                Data = quizzes,
                Message = $"Found {quizzes.Count} quizzes",
                Metadata = new Dictionary<string, object>
                {
                    { "total", total },
                    { "page", page },
                    { "pageSize", pageSize },
                    { "totalPages", (int)Math.Ceiling(total / (double)pageSize) }
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching public quizzes");
            return StatusCode(500, new ApiResponse<List<QuizDto>>
            {
                Success = false,
                Message = "Internal server error",
                Errors = new[] { ex.Message }
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<QuizDetailDto>>> GetQuiz(int id)
    {
        try
        {
            var quiz = await cacheService.GetOrSetQuizAsync(id, async () =>
                await dbContext.Quizzes
                    .Include(q => q.Questions!)
                        .ThenInclude(q => q.Answers)
                    .Include(q => q.OwnerUser)
                    .FirstOrDefaultAsync(q => q.QuizId == id && q.IsPublic)
            );

            if (quiz is null)
            {
                return NotFound(new ApiResponse<QuizDetailDto>
                {
                    Success = false,
                    Message = "Quiz not found"
                });
            }

            var dto = new QuizDetailDto
            {
                QuizId = quiz.QuizId,
                Title = quiz.Title,
                Category = quiz.Category,
                OwnerUsername = quiz.OwnerUser!.Username,
                CreatedAt = quiz.CreatedAt,
                IsPublic = quiz.IsPublic,
                ShuffleQuestions = quiz.ShuffleQuestions,
                Questions = quiz.Questions!.Select(q => new QuestionDto
                {
                    QuestionId = q.QuestionId,
                    Text = q.QuestionText,
                    TimeLimitSeconds = q.TimeLimitSeconds,
                    AnswerCount = q.Answers?.Count ?? 0
                }).ToList()
            };

            return Ok(new ApiResponse<QuizDetailDto>
            {
                Success = true,
                Data = dto
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching quiz {QuizId}", id);
            return StatusCode(500, new ApiResponse<QuizDetailDto>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<List<string>>>> GetCategories()
    {
        try
        {
            var categories = await dbContext.Quizzes
                .Where(q => q.IsPublic && !string.IsNullOrEmpty(q.Category))
                .Select(q => q.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(new ApiResponse<List<string>>
            {
                Success = true,
                Data = categories,
                Message = $"Found {categories.Count} categories"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching categories");
            return StatusCode(500, new ApiResponse<List<string>>
            {
                Success = false,
                Message = "Internal server error"
            });
        }
    }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string[]? Errors { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

public class QuizDto
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int QuestionCount { get; set; }
    public string OwnerUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }
}

public class QuizDetailDto
{
    public int QuizId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string OwnerUsername { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPublic { get; set; }
    public bool ShuffleQuestions { get; set; }
    public List<QuestionDto> Questions { get; set; } = [];
}

public class QuestionDto
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int TimeLimitSeconds { get; set; }
    public int AnswerCount { get; set; }
}
