using AlgoTrading.Application;
using AlgoTrading.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AlgoTrading API",
        Version = "v1",
        Description = "Algorithmic Trading System API for Korean Stock Market"
    });
});

// Add SignalR for real-time communication
builder.Services.AddSignalR();

// Add Application layer (CQRS, MediatR, FluentValidation, etc.)
builder.Services.AddApplication();

// Add Infrastructure layer (DbContext, Repositories, External Services, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// TODO: Add Authentication & Authorization
// builder.Services.AddAuthentication();
// builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AlgoTrading API V1");
        options.RoutePrefix = string.Empty; // Swagger UI at root
    });
}

app.UseHttpsRedirection();

// TODO: Add authentication middleware
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

// Map SignalR hubs
app.MapHub<AlgoTrading.API.Hubs.TradingHub>("/hubs/trading");

app.Run();
