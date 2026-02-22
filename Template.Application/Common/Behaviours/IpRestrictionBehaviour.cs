using Microsoft.Extensions.Hosting;
using Template.Application.Common.Exceptions;
using Template.Application.Common.Interfaces.Security;
using Template.Application.Common.Interfaces.Services;
using Template.Application.Common.Models;
using Template.Application.Common.Security;

namespace Template.Application.Common.Behaviours;

public class IpRestrictionBehaviour<TRequest, TResponse> where TResponse : notnull
{
    private readonly ITenantCacheService _cacheService;
    private readonly IHostEnvironment _env;
    private readonly ICurrentUser _user;

    public IpRestrictionBehaviour(ITenantCacheService cacheService, IHostEnvironment env, ICurrentUser user)
    {
        _cacheService = cacheService;
        _env = env;
        _user = user;
    }

    public async Task<ApiResponse<TResponse>> Handle(
        Func<Task<ApiResponse<TResponse>>> next,
        TRequest request,
        CancellationToken cancellationToken)
    {
        // Bypass para serviços internos (Core.Api)
        if (!string.IsNullOrEmpty(_user.GroupName) && _user.GroupName.Contains("Core.Api."))
            return await next();

        // Buscar IPs permitidos do tenant
        var allowedIps = await _cacheService.GetAllowedIpsAsync(
            _user.X_Tenant_ID, cancellationToken);

        // Se não há IPs configurados → acesso livre (sem restrição para ninguém)
        if (allowedIps == null || allowedIps.Count == 0)
            return await next();

        // Verificar se o request tem [BypassIpRestriction] → rota liberada
        var hasBypass = request!.GetType()
            .GetCustomAttributes(typeof(BypassIpRestrictionAttribute), true)
            .Any();

        if (hasBypass)
            return await next();

        // Bypass via token (usuário com BypassIp = true recebe claim scp=full)
        if (_user.Scp == "full")
            return await next();

        // Rota NÃO tem bypass → verificar IP do usuário
        var clientIp = _user.IpAddress;

        if (string.IsNullOrEmpty(clientIp))
        {
            if (_env.IsDevelopment())
                throw new ForbiddenAccessException("IP do cliente não identificado. Acesso negado pela restrição de IP.");

            throw new ForbiddenAccessException("Acesso negado.");
        }

        if (!allowedIps.Contains(clientIp))
        {
            if (_env.IsDevelopment())
                throw new ForbiddenAccessException($"IP '{clientIp}' não está na lista de IPs permitidos. Acesso negado.");

            throw new ForbiddenAccessException("Acesso negado.");
        }

        return await next();
    }
}