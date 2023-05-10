using amped_bookmark;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

DotEnv.Load();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddDbContext<BookmarkDb>(opt => opt.UseInMemoryDatabase("BookmarkList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetValue<string>("CLIENT_ORIGIN_URL") ?? string.Empty)
            .WithHeaders(new string[] {
                HeaderNames.ContentType,
                HeaderNames.Authorization,
            })
            .WithMethods("GET","POST")
            .SetPreflightMaxAge(TimeSpan.FromSeconds(86400));
    });
});

var issuer = $"https://{builder.Configuration.GetValue<string>("AUTH0_DOMAIN")}/" ;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var audience = builder.Configuration.GetValue<string>("AUTH0_AUDIENCE");
        options.Authority = issuer;
        options.Audience = audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("write:bookmarks", policy =>
    {
        policy.Requirements.Add(new
            HasScopeRequirement("write:bookmarks", issuer));
    });
});

builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

var app = builder.Build();

app.Urls.Add($"http://+:{app.Configuration.GetValue<string>("PORT")}");

var bookmarks = app.MapGroup("/bookmark");
bookmarks.MapGet("/", GetAllBookmarks);
bookmarks.MapGet("/unread", GetUnreadBookmarks);
bookmarks.MapPost("/", CreateUnreadBookmark).RequireAuthorization("write:bookmarks");

static async Task<IResult> GetAllBookmarks(BookmarkDb db)
{
    return TypedResults.Ok(await db.Bookmarks.ToArrayAsync());
}

static async Task<IResult> GetUnreadBookmarks(BookmarkDb db) {
    return TypedResults.Ok(await db.Bookmarks.Where(b => b.Read.Equals(false)).ToListAsync());
}

static async Task<IResult> CreateUnreadBookmark(ClaimsPrincipal user, BookmarkDto bookmark, BookmarkDb db)
{
    var newBookmark = Bookmark.CreateUnreadBookmark(bookmark.Uri, user.Identity.Name.ToString());
    
    db.Bookmarks.Add(newBookmark);
    await db.SaveChangesAsync();
   
    return TypedResults.Created($"/bookmark/{newBookmark.Id}", newBookmark);
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.Run();