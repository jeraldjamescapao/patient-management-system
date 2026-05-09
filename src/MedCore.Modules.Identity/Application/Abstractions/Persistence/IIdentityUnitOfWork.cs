namespace MedCore.Modules.Identity.Application.Abstractions.Persistence;

using Microsoft.EntityFrameworkCore.Storage;

internal interface IIdentityUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}