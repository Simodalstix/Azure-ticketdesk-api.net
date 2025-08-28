using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using TicketDesk.Api.Data;
using TicketDesk.Api.Models;
using Xunit;

namespace TicketDesk.Tests.Integration;

public class TicketApiTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithDatabase("ticketdesk_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TicketDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<TicketDbContext>(options =>
                        options.UseNpgsql(_postgres.GetConnectionString()));
                });
            });

        _client = _factory.CreateClient();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        _client.Dispose();
        await _factory.DisposeAsync();
    }

    [Fact]
    public async Task GetTickets_ReturnsEmptyList_WhenNoTickets()
    {
        var response = await _client.GetAsync("/api/tickets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResponse<TicketResponse>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateTicket_ReturnsCreated_WithValidData()
    {
        var request = new CreateTicketRequest("Test Ticket", "Test Description", TicketPriority.High);
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/tickets", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var responseContent = await response.Content.ReadAsStringAsync();
        var ticket = JsonSerializer.Deserialize<TicketResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        ticket!.Title.Should().Be("Test Ticket");
        ticket.Priority.Should().Be(TicketPriority.High);
    }

    [Fact]
    public async Task CreateTicket_ReturnsBadRequest_WithInvalidData()
    {
        var request = new CreateTicketRequest("", "Test Description", TicketPriority.High);
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/tickets", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}