using FluentValidation;
using HealthChecks.NpgSql;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using Serilog;
using Serilog.Events;
using System.Text.Json;
using TicketDesk.Api.Data;
using TicketDesk.Api.Models;
using TicketDesk.Api.Validators;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(new Serilog.Formatting.Json.JsonFormatter())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "Host=localhost;Database=ticketdesk;Username=postgres;Password=postgres";

builder.Services.AddDbContext<TicketDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTicketRequestValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TicketDesk API", Version = "v1" });
});

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
    };
});

app.UseHttpMetrics();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.MapControllers();
app.MapHealthChecks("/healthz");
app.MapMetrics("/metrics");

// Minimal API endpoints
app.MapGet("/api/tickets", async (TicketDbContext context, int page = 1, int pageSize = 10) =>
{
    var totalCount = await context.Tickets.CountAsync();
    var tickets = await context.Tickets
        .OrderByDescending(t => t.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(t => new TicketResponse(t.Id, t.Title, t.Description, t.Status, t.Priority, t.CreatedAt, t.UpdatedAt))
        .ToListAsync();

    return Results.Ok(new PagedResponse<TicketResponse>(tickets, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize)));
})
.WithName("GetTicketsMinimal")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get paginated tickets",
    Description = "Returns a paginated list of tickets"
});

app.MapPost("/api/tickets", async (CreateTicketRequest request, TicketDbContext context, IValidator<CreateTicketRequest> validator) =>
{
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
        return Results.ValidationProblem(validationResult.ToDictionary());

    var ticket = new Ticket { Title = request.Title, Description = request.Description, Priority = request.Priority };
    context.Tickets.Add(ticket);
    await context.SaveChangesAsync();
    
    return Results.Created($"/api/tickets/{ticket.Id}", new TicketResponse(ticket.Id, ticket.Title, ticket.Description, ticket.Status, ticket.Priority, ticket.CreatedAt, ticket.UpdatedAt));
})
.WithName("CreateTicketMinimal")
.WithOpenApi(operation => new(operation)
{
    Summary = "Create a new ticket",
    Description = "Creates a new support ticket"
});

// Global exception handler
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/problem+json";
        
        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "An error occurred while processing your request",
            status = 500,
            traceId = context.TraceIdentifier
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
    });
});

// Run migrations on startup if configured
if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();

public partial class Program { }