using MediatR;
using MyAi.Application.Features.Payment.Strategies;

namespace MyAi.Application.Features.Payment.Commands;

public record InitiatePaymentCommand(Guid PlanId) : IRequest<CheckoutData>;
