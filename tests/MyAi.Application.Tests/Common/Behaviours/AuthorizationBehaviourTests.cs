using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using MyAi.Application.Common.Behaviours;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Exceptions;
using MyAi.Domain.Enums;
using Xunit;

namespace MyAi.Application.Tests.Common.Behaviours;

public class AuthorizationBehaviourTests
{
    private record OpenRequest : IRequest<string>;

    private record AdminRequest : IRequest<string>, IRequireRole
    {
        public Domain.Enums.UserRole MinimumRole => Domain.Enums.UserRole.Admin;
    }

    private static ICurrentUserService CurrentUser(UserRole? role)
    {
        var mock = new Mock<ICurrentUserService>();
        mock.Setup(c => c.GetUserRole()).Returns(role);
        return mock.Object;
    }

    [Fact]
    public async Task Handle_RequestWithoutRoleRequirement_ShouldCallNext()
    {
        var logger = new Mock<ILogger<AuthorizationBehaviour<OpenRequest, string>>>();
        var behaviour = new AuthorizationBehaviour<OpenRequest, string>(
            CurrentUser(UserRole.PublicUser), logger.Object);

        var result = await behaviour.Handle(new OpenRequest(), () => Task.FromResult("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_AdminRequiredWithAdminRole_ShouldCallNext()
    {
        var logger = new Mock<ILogger<AuthorizationBehaviour<AdminRequest, string>>>();
        var behaviour = new AuthorizationBehaviour<AdminRequest, string>(
            CurrentUser(UserRole.Admin), logger.Object);

        var result = await behaviour.Handle(new AdminRequest(), () => Task.FromResult("ok"), CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_AdminRequiredWithSubscriberRole_ShouldThrowForbidden()
    {
        var logger = new Mock<ILogger<AuthorizationBehaviour<AdminRequest, string>>>();
        var behaviour = new AuthorizationBehaviour<AdminRequest, string>(
            CurrentUser(UserRole.Subscriber), logger.Object);

        var act = () => behaviour.Handle(new AdminRequest(), () => Task.FromResult("ok"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Handle_AnonymousUser_ShouldThrowForbidden()
    {
        var logger = new Mock<ILogger<AuthorizationBehaviour<AdminRequest, string>>>();
        var behaviour = new AuthorizationBehaviour<AdminRequest, string>(CurrentUser(null), logger.Object);

        var act = () => behaviour.Handle(new AdminRequest(), () => Task.FromResult("ok"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
