using MediatR;
using MyAi.Application.Common.Exceptions;
using MyAi.Application.Common.Interfaces;
using MyAi.Application.Common.Interfaces.Repositories;
using MyAi.Application.Features.CustomAi;

namespace MyAi.Application.Features.CustomAi.Queries;

public class TestCustomAiKeyQueryHandler : IRequestHandler<TestCustomAiKeyQuery, CustomAiKeyTestResultDto>
{
    private readonly ICustomAiConfigRepository _customAiConfigRepository;
    private readonly IApiKeyEncryptionService _apiKeyEncryptionService;
    private readonly IOpenRouterKeyTester _keyTester;
    private readonly ICurrentUserService _currentUserService;

    public TestCustomAiKeyQueryHandler(
        ICustomAiConfigRepository customAiConfigRepository,
        IApiKeyEncryptionService apiKeyEncryptionService,
        IOpenRouterKeyTester keyTester,
        ICurrentUserService currentUserService)
    {
        _customAiConfigRepository = customAiConfigRepository;
        _apiKeyEncryptionService = apiKeyEncryptionService;
        _keyTester = keyTester;
        _currentUserService = currentUserService;
    }

    public async Task<CustomAiKeyTestResultDto> Handle(TestCustomAiKeyQuery query, CancellationToken ct)
    {
        var userId = _currentUserService.GetUserId();
        var config = await _customAiConfigRepository.GetByUserIdAsync(userId, ct)
                     ?? throw new NotFoundException("No custom AI config found for the current user. Save a key first.");

        var apiKey = _apiKeyEncryptionService.Decrypt(config.EncryptedApiKey);
        var (isValid, errorDetail, modelInfo) = await _keyTester.TestKeyAsync(apiKey, config.PreferredModel, ct);

        if (isValid)
        {
            config.MarkVerified();
        }

        return new CustomAiKeyTestResultDto(isValid, errorDetail, modelInfo);
    }
}
