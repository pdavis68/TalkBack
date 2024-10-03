using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using TalkBack.Utility;
using TalkBackChatServer.Data;
using TalkBackChatServer.Models;
using TalkBackChatServer.Services;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
    builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add TalkBack
builder.Services.RegisterTalkBack(); 

// Add Configuration
builder.Services.Configure<LLMConfig>(builder.Configuration.GetSection("LLMConfig"));
builder.Services.AddSingleton<ILLMConfig>(provider =>
{
    var options = provider.GetRequiredService<IOptions<LLMConfig>>();
    return options.Value;
});

// Add services
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Use Serilog request logging
app.UseSerilogRequestLogging();

try
{
    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
