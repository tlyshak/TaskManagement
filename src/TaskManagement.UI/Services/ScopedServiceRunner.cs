using Microsoft.Extensions.DependencyInjection;

namespace TaskManagement.UI.Services;

public sealed class ScopedServiceRunner(IServiceScopeFactory scopeFactory)
{
    public async Task<TResult> RunAsync<TService, TResult>(Func<TService, Task<TResult>> action)
        where TService : notnull
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<TService>());
    }

    public async Task RunAsync<TService>(Func<TService, Task> action)
        where TService : notnull
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        await action(scope.ServiceProvider.GetRequiredService<TService>());
    }
}
