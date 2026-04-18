using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ITEC275LiveQuiz.Pages;

public class IndexModel : AppPageModel
{
    public bool IsLoggedIn { get; set; }

    public void OnGet()
    {
        IsLoggedIn = GetCurrentUserId().HasValue;
    }
}
