using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// add sqlite
builder.Services.AddDbContext<BookmarkContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BookmarkContext")));

//add auth0 authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.Authority = builder.Configuration["Auth0:Authority"];
    options.Audience = builder.Configuration["Auth0:Audience"];
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("write:bookmark", policy =>
    {
        var issuer = $"{builder.Configuration["Auth0:Authority"]}/";
        policy.Requirements.Add(new
            HasScopeRequirement("write:bookmark", issuer));
    });
});

builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();


var app = builder.Build();

//add auth0 authentication
app.UseAuthentication();
app.UseAuthorization();

// get a bookmark by id
app.MapGet("/bookmark", async (BookmarkContext db) =>
{
    var bookmarks = await db.Bookmarks.ToListAsync();
    return bookmarks;
});

//post a bookmark
app.MapPost("/bookmark", async (CreateBookmarkRequest request, ClaimsPrincipal user, BookmarkContext db) =>
{
    // map bookmark to domain object
    var bookmark = new Bookmark
    {
        Url = request.Uri,
        ProfileId = user.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)!.Value,
        Read = false
    };
    
    // use sqlite to store the bookmark
    db.Add(bookmark);
    await db.SaveChangesAsync();

    return bookmark;
}).RequireAuthorization("write:bookmark");

app.Run();

