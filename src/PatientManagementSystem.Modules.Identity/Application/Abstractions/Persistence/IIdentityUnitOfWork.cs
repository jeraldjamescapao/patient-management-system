namespace PatientManagementSystem.Modules.Identity.Application.Abstractions.Persistence;

using Microsoft.EntityFrameworkCore.Storage;

public interface IIdentityUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}