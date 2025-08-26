using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketDesk.Api.Data;
using TicketDesk.Api.Models;

namespace TicketDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly TicketDbContext _context;

    public TicketsController(TicketDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<PagedResponse<TicketResponse>>> GetTickets(int page = 1, int pageSize = 10)
    {
        var totalCount = await _context.Tickets.CountAsync();
        var tickets = await _context.Tickets
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TicketResponse(t.Id, t.Title, t.Description, t.Status, t.Priority, t.CreatedAt, t.UpdatedAt))
            .ToListAsync();

        return Ok(new PagedResponse<TicketResponse>(tickets, page, pageSize, totalCount, (int)Math.Ceiling(totalCount / (double)pageSize)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TicketResponse>> GetTicket(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        return ticket == null ? NotFound() : Ok(new TicketResponse(ticket.Id, ticket.Title, ticket.Description, ticket.Status, ticket.Priority, ticket.CreatedAt, ticket.UpdatedAt));
    }

    [HttpPost]
    public async Task<ActionResult<TicketResponse>> CreateTicket(CreateTicketRequest request)
    {
        var ticket = new Ticket { Title = request.Title, Description = request.Description, Priority = request.Priority };
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, new TicketResponse(ticket.Id, ticket.Title, ticket.Description, ticket.Status, ticket.Priority, ticket.CreatedAt, ticket.UpdatedAt));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTicket(int id, UpdateTicketRequest request)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        if (request.Title != null) ticket.Title = request.Title;
        if (request.Description != null) ticket.Description = request.Description;
        if (request.Status != null) ticket.Status = request.Status.Value;
        if (request.Priority != null) ticket.Priority = request.Priority.Value;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return NotFound();

        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}