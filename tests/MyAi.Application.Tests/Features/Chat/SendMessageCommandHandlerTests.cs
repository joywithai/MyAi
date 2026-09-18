using FluentAssertions;
using MediatR;
using Moq;
using MyAi.Application.Common.Ai;
using MyAi.Application.Common.Enums;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Chat.Commands;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;
using Xunit;

namespace MyAi.Application.Tests.Features.Chat;

public class SendMessageCommandHandlerTests
{
    private static Conversation NewConversation() => Conversation.Create(Guid.NewGuid());

    [Fact]
    public async Task Handle_NonOwnedConversation_ShouldThrowForbidden()
    {
        var conversation = NewConversation();
        var otherUserId = Guid.NewGuid();
        conversation.SetUserId(otherUserId);

        var conversationRepository = new Mock<IConversationRepository>(MockBehavior.Strict);
        conversationRepository
            .Setup(r => r.GetByIdAsync(conversation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

        var handler = CreateHandlerWithDefaults(conversationRepository: conversationRepository.Object);

        var act = () => handler.Handle(
            new SendMessageCommand(conversation.Id, "hello", "en"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    private static SendMessageCommandHandler CreateHandlerWithDefaults(
        IConversationRepository? conversationRepository = null)
    {
        var ownership = new Mock<IConversationOwnershipChecker>();
        ownership
            .Setup(o => o.IsOwnedByCurrentUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        return new SendMessageCommandHandler(
            new Mock<IUserRepository>().Object,
            conversationRepository ?? new Mock<IConversationRepository>().Object,
            new Mock<IMessageRepository>().Object,
            new Mock<IAiProviderFactory>().Object,
            new Mock<ITtsProviderFactory>().Object,
            new Mock<IStorageService>().Object,
            new Mock<ICacheService>().Object,
            new Mock<ICurrentUserService>().Object,
            new Mock<IMediator>().Object,
            ownership.Object,
            new Mock<Microsoft.Extensions.Logging.ILogger<SendMessageCommandHandler>>().Object);
    }
}
