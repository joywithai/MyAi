using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Common.Exceptions;
using Xunit;

namespace MyAi.Application.Tests.Common.Behaviours;

public class ValidationBehaviourTests
{
    private record TestRequest : IRequest<string>;

    private class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x).NotEmpty();
        }
    }

    [Fact]
    public async Task Handle_NoValidators_ShouldCallNext()
    {
        var behaviour = new ValidationBehaviour<TestRequest, string>(Array.Empty<IValidator<TestRequest>>());
        var called = false;

        var result = await behaviour.Handle(new TestRequest(), () => { called = true; return Task.FromResult("ok"); }, CancellationToken.None);

        called.Should().BeTrue();
        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_FailedValidation_ShouldThrowValidationExceptionWithErrors()
    {
        var validator = new TestRequestValidator();
        var behaviour = new ValidationBehaviour<TestRequest, string>(new IValidator<TestRequest>[] { validator });

        // TestRequest is a record (never null itself), so force failure via a null value validator instead
        var alwaysFail = new Mock<IValidator<TestRequest>>();
        alwaysFail
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(
                new[] { new FluentValidation.Results.ValidationFailure("Field", "must not be empty") }));

        var failingBehaviour = new ValidationBehaviour<TestRequest, string>(
            new IValidator<TestRequest>[] { alwaysFail.Object });

        var act = () => failingBehaviour.Handle(new TestRequest(), () => Task.FromResult("ok"), CancellationToken.None);

        await act.Should().ThrowAsync<MyAi.Application.Common.Exceptions.ValidationException>()
            .Where(e => e.Errors.ContainsKey("Field"));
    }
}
