using FluentAssertions;
using MyAi.Domain.Entities;
using MyAi.Domain.Enums;
using MyAi.Domain.Events;
using Xunit;

namespace MyAi.Domain.Tests.Entities;

public class ConversationTests
{
    [Fact]
    public void AddMessage_ShouldIncrementCount()
    {
        var conversation = Conversation.Create(Guid.NewGuid());
        var message = Message.CreateUserMessage(conversation.Id, "আপনি কেমন আছেন?", Language.Bn);

        conversation.AddMessage(message);

        conversation.MessageCount.Should().Be(1);
        conversation.DomainEvents.OfType<MessageSentEvent>().Should().ContainSingle();
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAt()
    {
        var conversation = Conversation.Create(Guid.NewGuid());

        conversation.SoftDelete();

        conversation.IsDeleted.Should().BeTrue();
        conversation.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Create_ShouldRaiseConversationStartedEvent()
    {
        var userId = Guid.NewGuid();

        var conversation = Conversation.Create(userId);

        conversation.DomainEvents.OfType<ConversationStartedEvent>()
            .Should().ContainSingle(e => e.UserId == userId);
    }
}
