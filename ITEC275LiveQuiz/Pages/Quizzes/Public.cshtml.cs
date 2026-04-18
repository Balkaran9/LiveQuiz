using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Models;
using ITEC275LiveQuiz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Pages.Quizzes;

public class PublicModel(
    AppDbContext dbContext, 
    CacheService cacheService,
    ILogger<PublicModel> logger) : PageModel
{
    public List<Quiz> Quizzes { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? CategoryFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "recent";

    public List<string> AvailableCategories { get; set; } = [];

    public async Task OnGetAsync()
    {
        logger.LogInformation("Loading public quizzes (search: {Search}, category: {Category}, sort: {Sort})",
            SearchTerm ?? "none", CategoryFilter ?? "all", SortBy);

        // Load categories
        AvailableCategories = await dbContext.Quizzes
            .Where(q => q.IsPublic && !string.IsNullOrEmpty(q.Category))
            .Select(q => q.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        // Build query
        var query = dbContext.Quizzes.Where(q => q.IsPublic);

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            query = query.Where(q => q.Title.Contains(SearchTerm) || q.OwnerUser!.Username.Contains(SearchTerm));
        }

        if (!string.IsNullOrWhiteSpace(CategoryFilter) && CategoryFilter != "all")
        {
            query = query.Where(q => q.Category == CategoryFilter);
        }

        // Apply sorting
        query = SortBy switch
        {
            "title" => query.OrderBy(q => q.Title),
            "popular" => query.OrderByDescending(q => q.LiveGames!.Count),
            "questions" => query.OrderByDescending(q => q.Questions!.Count),
            _ => query.OrderByDescending(q => q.CreatedAt)
        };

        Quizzes = await query
            .Include(q => q.Questions)
            .Include(q => q.OwnerUser)
            .Include(q => q.LiveGames)
            .ToListAsync();

        logger.LogInformation("Loaded {Count} public quizzes", Quizzes.Count);
    }
}
