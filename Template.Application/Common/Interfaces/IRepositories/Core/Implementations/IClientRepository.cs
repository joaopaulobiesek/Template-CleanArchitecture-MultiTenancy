using Template.Application.Common.Interfaces.IRepositories.Core.Base;
using Template.Domain.Entity.Core;

namespace Template.Application.Common.Interfaces.IRepositories.Core.Implementations;

public interface IClientRepository : IRepository<Client>
{
    IQueryable<Client> SearchIQueryable(string? src);
}