namespace PatientManagementSystem.Modules.Identity.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore.Storage;
using PatientManagementSystem.Modules.Identity.Application.Abstractions.Persistence;

internal sealed class IdentityUnitOfWork : IIdentityUnitOfWork
{
    private readonly IdentityDbContext _context;

    public IdentityUnitOfWork(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        return await _context.Database.BeginTransactionAsync(ct);
    }
}