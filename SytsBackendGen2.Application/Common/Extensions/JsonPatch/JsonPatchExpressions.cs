using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Adapters;
using Microsoft.AspNetCore.JsonPatch.Exceptions;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Application.Common.Exceptions;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.Common.Extensions.StringExtensions;
using SytsBackendGen2.Domain.Common;
using Newtonsoft.Json.Serialization;
using SytsBackendGen2.Application.Extensions.DataBaseProvider;

namespace SytsBackendGen2.Application.Extensions.JsonPatch;

internal static class JsonPatchExpressions
{
    private static IObjectAdapter? _adapter;

    public static IObjectAdapter Adapter
    {
        get
        {
            if (_adapter == null)
            {
                IAdapterFactory factory = CustomAdapterFactory.Default;
                _adapter = new ObjectAdapter(new DefaultContractResolver(), null, factory);
            }
            return _adapter;
        }
    }

    /// <summary>
    /// Deprecated.
    /// </summary>
    internal static TDestination? ApplyToSource<TDto, TDestination>
        (this JsonPatchDocument<TDto> patch, TDestination? destination, IMapper mapper)
        where TDto : class, IBaseDto
        where TDestination : BaseEntity
    {
        var dto = mapper.Map<TDto>(destination);
        patch.ApplyTo(dto, Adapter);
        mapper.Map(dto, destination);
        return destination;
    }

    /// <summary>
    /// Applies json patch to the database.
    /// </summary>
    /// <typeparam name="TDbSet">Type of DbSet to apply json patch to.</typeparam>
    /// <typeparam name="TDestination">Type of entity.</typeparam>
    /// <param name="patch">Json patch document containing operations</param>
    /// <param name="dbSet">DbSet to apply json patch to.</param>
    internal static void ApplyTransactionToSource<TDestination>
        (this JsonPatchDocument<DbSet<TDestination>> patch, DbSet<TDestination> dbSet)
        where TDestination : BaseEntity
    {
        IDbContext context = dbSet.GetContext();
        using (var transaction = context.Database.BeginTransaction())
        {
            int operationIndex = 0;
            try
            {
                while (operationIndex < patch.Operations.Count)
                {
                    patch.Operations[operationIndex].Apply(dbSet, Adapter);
                    operationIndex++;
                }
                //patch.ApplyTo(dbSet, Adapter);
                transaction.Commit();
            }
            catch (JsonPatchException ex)
            {
                transaction.Rollback();
                if (ex.FailedOperation != null)
                {
                    throw new JsonPatchExceptionWithPosition(
                        $"{(ex.FailedOperation as IDbSetOperation).dtoPath}: {ex.Message}", 
                        ex,
                        operationIndex);
                }
                throw;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    /// <summary>
    /// Converts json patch document from <typeparamref name="TDto"/> to DbSet of <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TDestination">Entity type.</typeparam>
    /// <typeparam name="TDto">DTO type.</typeparam>
    /// <param name="patch">Json patch document containing operations.</param>
    /// <param name="provider">Configuraion provider for performing maps.</param>
    /// <returns>Json patch document of DbSet of <typeparamref name="TDestination"/></returns>
    internal static JsonPatchDocument<DbSet<TDestination>> ConvertToSourceDbSet
        <TDto, TDestination>(this JsonPatchDocument<TDto> patch, IConfigurationProvider provider)
        where TDestination : BaseEntity
        where TDto : class, IEditDto
    {
        var newOperations = new List<Operation<DbSet<TDestination>>>();
        foreach (var operation in patch.Operations)
        {
            var jsonPatchPath = new JsonPatchPath(operation.path);

            var newOperation = new DbSetOperation<TDestination>()
            {
                dtoPath = operation.path,
                from = operation.from,
                op = operation.op,
            };

            try
            {
                newOperation.path =
                    DtoExtension.GetSourceJsonPatch<TDto>(
                        jsonPatchPath.AsSingleProperty,
                        provider,
                        out var propertyPathTypes);
                newOperation.path = jsonPatchPath.ToFullPropertyPath(newOperation.path);

                if (newOperation.OperationType != OperationType.Remove)
                {
                    newOperation.value =
                        DtoExtension.GetSourceValueJsonPatch(
                            operation.value,
                            propertyPathTypes,
                            provider,
                            jsonPatchPath.LastSegment);
                }
            }
            catch (Exception ex)
            {
                throw new JsonPatchException($"{newOperation.dtoPath}: {ex.Message}", ex);
            }

            newOperations.Add(newOperation);
        }
        return new JsonPatchDocument<DbSet<TDestination>>(
            newOperations,
            new CamelCasePropertyNamesContractResolver());
    }

    /// <summary>
    /// Converts operations from <typeparamref name="TDto"/> to DbSet of <typeparamref name="TDestination"/> and applies them to database.
    /// </summary>
    /// <typeparam name="TDestination">Type of database entity.</typeparam>
    /// <typeparam name="TDto">Type of DTO.</typeparam>
    /// <param name="patch">Json patch document containing operations.</param>
    /// <param name="dbSet">DbSet to apply json patch to.</param>
    /// <param name="provider">Configuraion provider for performing maps.</param>
    internal static void ApplyDtoTransactionToSource
        <TDestination, TDto>(
        this JsonPatchDocument<TDto> patch,
        DbSet<TDestination> dbSet,
        IConfigurationProvider provider)
        where TDestination : BaseEntity
        where TDto : class, IEditDto
    {
        var convertedPatch = patch.ConvertToSourceDbSet<TDto, TDestination>(provider);
        convertedPatch.ApplyTransactionToSource(dbSet);
    }

    private static int GetLength(this int value)
    {
        int len = value >= 0 ? 1 : 2;
        while (value > 10 || value < -10)
        {
            value /= 10;
            len++;
        }
        return len;
    }
}
