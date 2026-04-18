using Microsoft.AspNetCore.Mvc;

namespace ITEC275LiveQuiz.Pages.Account;

public class LogoutModel : ITEC275LiveQuiz.Pages.AppPageModel
{
    public IActionResult OnGet()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Index");
    }

    public IActionResult OnPost()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Index");
    }
}
