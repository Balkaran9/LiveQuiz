using ITEC275LiveQuiz.Data;
using Microsoft.EntityFrameworkCore;

namespace ITEC275LiveQuiz.Services;

public class JoinCodeService(AppDbContext dbContext)
{
    private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public async Task<string> GenerateUniqueCodeAsync()
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var code = new string(Enumerable.Range(0, 6)
                .Select(_ => Characters[Random.Shared.Next(Characters.Length)])
                .ToArray());

            var exists = await dbContext.LiveGames.AnyAsync(g => g.JoinCode == code);
            if (!exists)
            {
                return code;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique join code.");
    }
}
