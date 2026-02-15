using API.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public abstract class BaseService
{
    protected readonly AppDbContext Db;

    protected BaseService(AppDbContext db)
    {
        Db = db;
    }

    /// <summary>
    /// Executes a database operation with error handling.
    /// </summary>
    protected async Task<ServiceResult<T>> ExecuteDatabaseOperationAsync<T>(
        Func<Task<T>> operation,
        string errorMessage = "Database operation failed")
    {
        try
        {
            var result = await operation();
            return ServiceResult<T>.Ok(result);
        }
        catch (DbUpdateException ex)
        {
            return ServiceResult<T>.Fail(ServiceErrorType.Database, $"{errorMessage}: {ex.InnerException?.Message ?? ex.Message}");
        }
        catch (Exception ex)
        {
            return ServiceResult<T>.Fail(ServiceErrorType.Unknown, $"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Executes a query operation with error handling.
    /// </summary>
    protected async Task<ServiceResult<T>> ExecuteQueryAsync<T>(
        Func<Task<T>> query,
        string errorMessage = "Failed to retrieve data")
    {
        try
        {
            var result = await query();
            return ServiceResult<T>.Ok(result);
        }
        catch (Exception ex)
        {
            return ServiceResult<T>.Fail(ServiceErrorType.Database, $"{errorMessage}: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates if a string is not null or whitespace.
    /// </summary>
    protected string? ValidateNotEmpty(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return $"{fieldName} cannot be empty or contain only whitespace.";
        return null;
    }

    /// <summary>
    /// Checks if an entity exists and returns a NotFound result if not.
    /// </summary>
    protected async Task<ServiceResult<T>?> ValidateEntityExists<T, TEntity>(
        int id,
        Func<Task<TEntity?>> getEntity,
        string entityName) where TEntity : class
    {
        var entity = await getEntity();
        if (entity == null)
            return ServiceResult<T>.NotFound();
        return null;
    }

    /// <summary>
    /// Validates the result of a custom validation method.
    /// </summary>
    protected ServiceResult<T>? ValidateOrFail<T>(string? validationError)
    {
        if (validationError != null)
            return ServiceResult<T>.Fail(ServiceErrorType.Validation, validationError);
        return null;
    }

    /// <summary>
    /// Checks for entity relationships before deletion.
    /// </summary>
    protected ServiceResult<T>? ValidateNoDependencies<T>(
        bool hasDependencies,
        string entityName,
        string dependencyName,
        int dependencyCount)
    {
        if (hasDependencies)
        {
            return ServiceResult<T>.Fail(
                ServiceErrorType.Validation,
                $"Cannot delete {entityName} because it has {dependencyCount} associated {dependencyName}."
            );
        }
        return null;
    }
}
