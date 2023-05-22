using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

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

//post a bookmark
app.MapPost("/bookmark", (CreateBookmarkRequest bookmark) =>
{
    return bookmark;
}).RequireAuthorization("write:bookmark");

app.MapGet("/bookmark", () => new List<Bookmark>
{
    new Bookmark { Url = "https://www.google.com", ProfileId = 1, Read = false },
    new Bookmark { Url = "https://www.bing.com", ProfileId = 2, Read = true },
    new Bookmark { Url = "https://www.yahoo.com", ProfileId = 3, Read = false }
});
app.Run();
