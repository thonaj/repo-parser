using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RepoParser.Api.Hubs;
using RepoParser.Core.Interfaces;
using RepoParser.Infrastructure.Data;
using RepoParser.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
//add a comment to trigger pr
// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Database
builder.Services.AddDbContext<RepoDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=repoparser.db"));

// Application services
builder.Services.AddScoped<IRepositoryService, RepositoryService>();
builder.Services.AddScoped<IAstParserService, AstParserService>();
builder.Services.AddScoped<IDriftDetectionService, DriftDetectionService>();
builder.Services.AddScoped<IDependencyGraphService, DependencyGraphService>();
builder.Services.AddSingleton<IFileWatcherService, FileWatcherService>();

// CORS for Vue.js frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("VueApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RepoDbContext>();
    db.Database.EnsureCreated();
}

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("VueApp");
app.UseAuthorization();
app.MapControllers();
app.MapHub<AlertHub>("/hubs/alerts");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
