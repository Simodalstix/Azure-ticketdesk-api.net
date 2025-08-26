using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketDesk.Api.Data;
using TicketDesk.Api.Models;

namespace TicketDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly TicketDbContext _context;

    public CommentsController(TicketDbContext context) => _context = context;

    [HttpGet("ticket/{ticketId}")]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetCommentsByTicket(int ticketId)
    {
        if (!await _context.Tickets.AnyAsync(t => t.Id == ticketId))
            return NotFound("Ticket not found");

        var comments = await _context.Comments
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentResponse(c.Id, c.TicketId, c.Content, c.CreatedAt))
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPost("ticket/{ticketId}")]
    public async Task<ActionResult<CommentResponse>> CreateComment(int ticketId, CreateCommentRequest request)
    {
        if (!await _context.Tickets.AnyAsync(t => t.Id == ticketId))
            return NotFound("Ticket not found");

        var comment = new Comment { TicketId = ticketId, Content = request.Content };
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCommentsByTicket), new { ticketId }, new CommentResponse(comment.Id, comment.TicketId, comment.Content, comment.CreatedAt));
    }
}