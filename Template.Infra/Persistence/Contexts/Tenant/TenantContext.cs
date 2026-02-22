using Microsoft.EntityFrameworkCore.ChangeTracking;
using Template.Domain.Entity;

namespace Template.Infra.Persistence.Contexts.Tenant
{
    public class TenantContext : BaseContext, ITenantContext
    {

        private string _connectionString;

        public TenantContext(DbContextOptions<TenantContext> options) : base(options)
        {
        }

        public new DbSet<T> Set<T>() where T : class => base.Set<T>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var assembly = Assembly.GetExecutingAssembly();
            var mapsNamespace = $"{GetType().Namespace.Replace(".Persistence.Contexts.Tenant", ".Settings.Maps.Tenant")}";

            var configurations = assembly.GetTypes()
                .Where(t => t.Namespace != null && t.Namespace.StartsWith(mapsNamespace))
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
                .ToList();

            foreach (var configuration in configurations)
            {
                var instance = Activator.CreateInstance(configuration);
                if (instance != null)
                {
                    modelBuilder.ApplyConfiguration((dynamic)instance);
                }
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrEmpty(_connectionString))
            {
                optionsBuilder.UseSqlServer(_connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.CommandTimeout(360); // 6 minutos - igual ao BFS
                });
            }
        }

        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
            Database.SetConnectionString(connectionString);
        }

        public string GetCurrentConnectionString()
        {
            return _connectionString;
        }

        public async Task ApplyMigrations()
        {
            if (Database.GetPendingMigrations().Any())
            {
                await Database.MigrateAsync();
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInformation();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditInformation()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Entity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (Entity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.Updated();
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.Updated();

                    var activeProperty = entry.Property(nameof(Entity.Active));
                    if (activeProperty.IsModified && !entity.Active)
                    {
                        entity.Delete();
                    }
                }
            }
        }
    }
}