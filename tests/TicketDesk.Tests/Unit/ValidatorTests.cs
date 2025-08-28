using FluentAssertions;
using TicketDesk.Api.Models;
using TicketDesk.Api.Validators;

namespace TicketDesk.Tests.Unit;

public class ValidatorTests
{
    [Fact]
    public void CreateTicketRequest_ValidData_ShouldPass()
    {
        var validator = new CreateTicketRequestValidator();
        var request = new CreateTicketRequest("Valid Title", "Valid Description", TicketPriority.High);

        var result = validator.Validate(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateTicketRequest_EmptyTitle_ShouldFail()
    {
        var validator = new CreateTicketRequestValidator();
        var request = new CreateTicketRequest("", "Valid Description", TicketPriority.High);

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void CreateTicketRequest_TitleTooLong_ShouldFail()
    {
        var validator = new CreateTicketRequestValidator();
        var request = new CreateTicketRequest(new string('x', 201), "Valid Description", TicketPriority.High);

        var result = validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }
}