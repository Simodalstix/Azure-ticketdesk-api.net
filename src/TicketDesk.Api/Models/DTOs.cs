namespace TicketDesk.Api.Models;

public record CreateTicketRequest(string Title, string Description, TicketPriority Priority = TicketPriority.Medium);
public record UpdateTicketRequest(string? Title, string? Description, TicketStatus? Status, TicketPriority? Priority);
public record CreateCommentRequest(string Content);

public record TicketResponse(int Id, string Title, string Description, TicketStatus Status, TicketPriority Priority, DateTime CreatedAt, DateTime UpdatedAt);
public record CommentResponse(int Id, int TicketId, string Content, DateTime CreatedAt);

public record PagedResponse<T>(IEnumerable<T> Items, int Page, int PageSize, int TotalCount, int TotalPages);