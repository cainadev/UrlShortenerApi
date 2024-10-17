using Microsoft.EntityFrameworkCore;
using UrlShortener;
using UrlShortener.Entities;
using UrlShortener.Models;
using UrlShortener.Services;
using UrlShortener.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Database")));

builder.Services.AddScoped<UrlShorteningService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyMigrations();
}

app.MapPost("shorten", async (
    ShortenUrlRequest request,
    UrlShorteningService urlShorteningService,
    ApplicationDbContext dbContext,
    HttpContext httpContext) =>
{
    if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
    {
        return Results.BadRequest("The Specified URL is invalid");
    }
    var code = await urlShorteningService.GenerateUniqueCode();

    var shortenedUrl = new ShortenedUrl
    {
        Id = Guid.NewGuid(),
        longUrl = request.Url,
        Code = code,
        ShortUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{code}",
        CreatedOnUtc = DateTime.Now
    };

    dbContext.ShortenedUrls.Add(shortenedUrl);

    await dbContext.SaveChangesAsync();

    return Results.Ok(shortenedUrl.ShortUrl);
});

app.MapGet("{code}", async (string code, ApplicationDbContext dbContext) =>
{
    var shortenedUrl = await dbContext.ShortenedUrls.FirstOrDefaultAsync(s => s.Code == code);

    if (shortenedUrl is null)
    {
        return Results.NotFound();
    }

    return Results.Redirect(shortenedUrl.longUrl);
});


app.UseHttpsRedirection();

app.Run();
