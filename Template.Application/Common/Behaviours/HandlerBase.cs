using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Models;
using Template.Application.Common.Persistence;

namespace Template.Application.Common.Behaviours;

public interface IHandlerBase<TRequest, TResponse> where TResponse : notnull
{
    Task<ApiResponse<TResponse>> Execute(TRequest request, CancellationToken cancellationToken, object? additionalData = null);
}

public abstract class HandlerBase<TRequest, TResponse> : IHandlerBase<TRequest, TResponse>
    where TResponse : notnull
{
    private ICoreContext Context;
    private ITenantContext TenantContext;
    private IServiceProvider _serviceProvider;
    private readonly ILogger<TRequest> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IIdentityService _identityService;
    private readonly PerformanceBehaviour<TRequest, TResponse> _performanceBehaviour;
    private readonly UnhandledExceptionBehaviour<TRequest, TResponse> _exceptionBehaviour;
    private readonly ValidationBehaviour<TRequest, TResponse> _validationBehaviour;
    private readonly AuthorizationBehaviour<TRequest, TResponse> _authorizationBehaviour;
    private readonly LoggingBehaviour<TRequest> _loggingBehaviour;

    protected ITenantContext _tenantContext => TenantContext ??= _serviceProvider.GetRequiredService<ITenantContext>();
    protected ICoreContext _context => Context ??= _serviceProvider.GetRequiredService<ICoreContext>();
    protected ICurrentUser _user;

    protected HandlerBase(HandlerDependencies<TRequest, TResponse> dependencies)
    {
        _serviceProvider = dependencies.ServiceProvider;
        _logger = dependencies.Logger;
        _user = dependencies.User;
        _identityService = dependencies.IdentityService;
        _environment = dependencies.Environment;

        _authorizationBehaviour = new AuthorizationBehaviour<TRequest, TResponse>(_user, _identityService, _environment);
        _performanceBehaviour = new PerformanceBehaviour<TRequest, TResponse>(_logger, _user, _identityService);
        _exceptionBehaviour = new UnhandledExceptionBehaviour<TRequest, TResponse>(_logger);
        _validationBehaviour = new ValidationBehaviour<TRequest, TResponse>(dependencies.Validators);
        _loggingBehaviour = new LoggingBehaviour<TRequest>(_logger, _user, _identityService);
    }

    protected abstract Task<ApiResponse<TResponse>> RunCore(TRequest request, CancellationToken cancellationToken, object? additionalData);

    public async Task<ApiResponse<TResponse>> Execute(TRequest request, CancellationToken cancellationToken, object? additionalData = null)
    {
        await _loggingBehaviour.Process(request, cancellationToken);

        return await _performanceBehaviour.Handle(
            () => _exceptionBehaviour.Handle(
                () => _validationBehaviour.Handle(
                    () => _authorizationBehaviour.Handle(
                        () => RunCore(request, cancellationToken, additionalData),
                        request, cancellationToken),
                    request, cancellationToken),
                request, cancellationToken),
            request, cancellationToken);
    }
}