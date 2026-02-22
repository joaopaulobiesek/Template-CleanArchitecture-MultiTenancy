using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Template.Application.Common.Interfaces.IRepositories.Core.Implementations;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Interfaces.Services;
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
    private readonly UnhandledExceptionBehaviour<TRequest, TResponse> _exceptionBehaviour;
    private readonly ValidationBehaviour<TRequest, TResponse> _validationBehaviour;
    private readonly AuthorizationBehaviour<TRequest, TResponse> _authorizationBehaviour;
    private readonly ModuleValidationBehaviour<TRequest, TResponse> _moduleValidationBehaviour;
    private readonly IpRestrictionBehaviour<TRequest, TResponse> _ipRestrictionBehaviour;
    private readonly AuditBehaviour<TRequest, TResponse> _auditBehaviour;

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
        _exceptionBehaviour = new UnhandledExceptionBehaviour<TRequest, TResponse>(_logger);
        _validationBehaviour = new ValidationBehaviour<TRequest, TResponse>(dependencies.Validators);
        _moduleValidationBehaviour = new ModuleValidationBehaviour<TRequest, TResponse>(_serviceProvider.GetRequiredService<IClientRepository>(), _environment, _user);

        // AuditBehaviour substitui LoggingBehaviour e PerformanceBehaviour
        // Envolve toda a execução para capturar sucesso/erro e duração
        var auditService = _serviceProvider.GetRequiredService<IAuditService>();
        var tenantCacheService = _serviceProvider.GetRequiredService<ITenantCacheService>();
        _auditBehaviour = new AuditBehaviour<TRequest, TResponse>(_logger, _user, auditService, tenantCacheService);
        _ipRestrictionBehaviour = new IpRestrictionBehaviour<TRequest, TResponse>(tenantCacheService, _environment, _user);
    }

    protected abstract Task<ApiResponse<TResponse>> RunCore(TRequest request, CancellationToken cancellationToken, object? additionalData);

    /// <summary>
    /// Pipeline de execução:
    /// AuditBehaviour (envolve tudo - captura sucesso/erro/duracao)
    ///   -> ExceptionBehaviour (try/catch)
    ///     -> ValidationBehaviour (FluentValidation)
    ///       -> AuthorizationBehaviour ([Authorize])
    ///         -> ModuleValidationBehaviour ([RequiresModule])
    ///           -> IpRestrictionBehaviour (bloqueia por padrão, [BypassIpRestriction] libera)
    ///             -> RunCore()
    /// </summary>
    public async Task<ApiResponse<TResponse>> Execute(TRequest request, CancellationToken cancellationToken, object? additionalData = null)
    {
        return await _auditBehaviour.Handle(
            () => _exceptionBehaviour.Handle(
                () => _validationBehaviour.Handle(
                    () => _authorizationBehaviour.Handle(
                        () => _moduleValidationBehaviour.Handle(
                            () => _ipRestrictionBehaviour.Handle(
                                () => RunCore(request, cancellationToken, additionalData),
                                request, cancellationToken),
                            request, cancellationToken),
                        request, cancellationToken),
                    request, cancellationToken),
                request, cancellationToken),
            request, cancellationToken);
    }
}