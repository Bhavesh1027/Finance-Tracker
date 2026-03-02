using FinanceTracker.Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using Xunit;

namespace FinanceTracker.UnitTests;

public sealed class ValidationBehaviorTests
{
    private sealed record TestRequest(string Name) : IRequest<string>;

    private sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    [Fact]
    public async Task Should_ThrowValidationException_WhenValidatorFails()
    {
        var validators = new[] { new TestRequestValidator() };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);

        var request = new TestRequest(string.Empty);
        var next = new Mock<RequestHandlerDelegate<string>>();

        Func<Task> act = async () => await behavior.Handle(request, next.Object, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        next.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task Should_CallNext_WhenValidationPasses()
    {
        var validators = new[] { new TestRequestValidator() };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);

        var request = new TestRequest("Valid");

        var next = new Mock<RequestHandlerDelegate<string>>();
        next.Setup(n => n()).ReturnsAsync("OK");

        var response = await behavior.Handle(request, next.Object, CancellationToken.None);

        response.Should().Be("OK");
        next.Verify(n => n(), Times.Once);
    }
}

