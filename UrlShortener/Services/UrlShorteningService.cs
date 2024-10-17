using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Services;

public class UrlShorteningService
{
    public const int NumberOfCharsInShortLink = 7;
    private const string AlphaBet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private readonly Random _random = new();
    private readonly ApplicationDbContext _context;

    public UrlShorteningService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<string> GenerateUniqueCode()
    {
        var codeChars = new char[NumberOfCharsInShortLink];

        while (true)
        {
            for (int i = 0; i < NumberOfCharsInShortLink; i++)
            {
                int randomIndex = _random.Next(AlphaBet.Length - 1);

                codeChars[i] = AlphaBet[randomIndex];
            }

            var code = new string(codeChars);

            if (!await _context.ShortenedUrls.AnyAsync(s => s.Code == code))
            {
                return code;
            }
        }

    }
}
