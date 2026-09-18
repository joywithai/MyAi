using AutoMapper;
using MediatR;
using MyAi.Application.Common.Constants;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.Avatar;

namespace MyAi.Application.Features.Admin.Commands;

public class CreateExpressionCommandHandler : IRequestHandler<CreateExpressionCommand, ExpressionDto>
{
    private readonly IExpressionRepository _expressionRepository;
    private readonly IMapper _mapper;

    public CreateExpressionCommandHandler(IExpressionRepository expressionRepository, IMapper mapper)
    {
        _expressionRepository = expressionRepository;
        _mapper = mapper;
    }

    public async Task<ExpressionDto> Handle(CreateExpressionCommand command, CancellationToken ct)
    {
        var expression = Domain.Entities.Expression.Create(
            command.Name, command.DisplayName, RoleConstants.ToUserRole(command.MinRole));

        expression.SetDescription(command.Description);
        await _expressionRepository.AddAsync(expression, ct);

        return _mapper.Map<ExpressionDto>(expression);
    }
}

public class UpdateExpressionCommandHandler : IRequestHandler<UpdateExpressionCommand, ExpressionDto>
{
    private readonly IExpressionRepository _expressionRepository;
    private readonly IMapper _mapper;

    public UpdateExpressionCommandHandler(IExpressionRepository expressionRepository, IMapper mapper)
    {
        _expressionRepository = expressionRepository;
        _mapper = mapper;
    }

    public async Task<ExpressionDto> Handle(UpdateExpressionCommand command, CancellationToken ct)
    {
        var expression = await _expressionRepository.GetByIdAsync(command.Id, ct)
                         ?? throw new NotFoundException("Expression", command.Id);

        if (command.DisplayName is not null || command.Description is not null)
        {
            expression.SetDescription(command.Description);
        }

        if (!string.IsNullOrWhiteSpace(command.MinRole))
        {
            expression.UpdateMinRole(RoleConstants.ToUserRole(command.MinRole));
        }

        if (command.IsActive == true)
        {
            expression.Activate();
        }
        else if (command.IsActive == false)
        {
            expression.Deactivate();
        }

        await _expressionRepository.UpdateAsync(expression, ct);

        return _mapper.Map<ExpressionDto>(expression);
    }
}

public class CreateAnimationCommandHandler : IRequestHandler<CreateAnimationCommand, AnimationDto>
{
    private readonly IAnimationRepository _animationRepository;
    private readonly IMapper _mapper;

    public CreateAnimationCommandHandler(IAnimationRepository animationRepository, IMapper mapper)
    {
        _animationRepository = animationRepository;
        _mapper = mapper;
    }

    public async Task<AnimationDto> Handle(CreateAnimationCommand command, CancellationToken ct)
    {
        var animation = Domain.Entities.Animation.Create(
            command.Name, command.DisplayName, RoleConstants.ToUserRole(command.MinRole));

        animation.SetDescription(command.Description);
        await _animationRepository.AddAsync(animation, ct);

        return _mapper.Map<AnimationDto>(animation);
    }
}

public class UpdateAnimationCommandHandler : IRequestHandler<UpdateAnimationCommand, AnimationDto>
{
    private readonly IAnimationRepository _animationRepository;
    private readonly IMapper _mapper;

    public UpdateAnimationCommandHandler(IAnimationRepository animationRepository, IMapper mapper)
    {
        _animationRepository = animationRepository;
        _mapper = mapper;
    }

    public async Task<AnimationDto> Handle(UpdateAnimationCommand command, CancellationToken ct)
    {
        var animation = await _animationRepository.GetByIdAsync(command.Id, ct)
                        ?? throw new NotFoundException("Animation", command.Id);

        if (command.DisplayName is not null || command.Description is not null)
        {
            animation.SetDescription(command.Description);
        }

        if (!string.IsNullOrWhiteSpace(command.MinRole))
        {
            animation.UpdateMinRole(RoleConstants.ToUserRole(command.MinRole));
        }

        if (command.IsActive == true)
        {
            animation.Activate();
        }
        else if (command.IsActive == false)
        {
            animation.Deactivate();
        }

        await _animationRepository.UpdateAsync(animation, ct);

        return _mapper.Map<AnimationDto>(animation);
    }
}
