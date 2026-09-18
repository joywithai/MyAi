namespace MyAi.Application.Common.Interfaces;

public interface ITtsProviderFactory
{
    ITtsProvider Create(string providerName);
}
