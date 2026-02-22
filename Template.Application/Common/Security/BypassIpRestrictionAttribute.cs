namespace Template.Application.Common.Security;

/// <summary>
/// Marca um Command/Query como isento da restrição de IP.
/// Por padrão, quando o tenant tem IPs configurados, TODAS as rotas são bloqueadas.
/// Adicionar este atributo libera a rota para acesso sem verificação de IP.
/// Se o tenant não tiver IPs configurados, nenhuma rota é bloqueada (acesso livre).
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class BypassIpRestrictionAttribute : Attribute
{
}
