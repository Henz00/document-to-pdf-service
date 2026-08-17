using ODTDOCXtoPDFConverter.Api.Services;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Identity;
using ODTDOCXtoPDFConverter.Api.Data;
using Microsoft.EntityFrameworkCore;
using ODTDOCXtoPDFConverter.Api.Models;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<AddUserService>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

if (args.Length > 0 && args[0] == "create-user")
{
    if (args.Length < 2)
    {
        Console.WriteLine("Usage: create-user <username>");
        return;
    }

    string username = args[1];

    Console.Write("Password: ");
    string? password = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(password))
    {
        Console.WriteLine("Password cannot be empty.");
        return;
    }

    using var scope = app.Services.CreateScope();

    var addUserService = scope.ServiceProvider
        .GetRequiredService<AddUserService>();

    await addUserService.AddUserAsync(username, password);

    return;
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
