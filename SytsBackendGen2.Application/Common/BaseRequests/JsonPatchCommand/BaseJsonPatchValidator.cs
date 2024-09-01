using AutoMapper;
using AutoMapper.Internal;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch.Operations;
using SytsBackendGen2.Application.Common.Dtos;
using SytsBackendGen2.Application.Extensions.JsonPatch;
using System.Collections;
using System.Reflection;

namespace SytsBackendGen2.Application.Common.BaseRequests.JsonPatchCommand;

public abstract class BaseJsonPatchValidator<TCommand, TResponse, TDto> : AbstractValidator<TCommand>
    where TCommand : BaseJsonPatchCommand<TResponse, TDto>
    where TResponse : BaseResponse
    where TDto : class, IEditDto, new()
{
    public BaseJsonPatchValidator(IMapper mapper)
    {
        RuleForEach(x => x.Patch.Operations).NotNull()
            .ValidateOperations<TCommand, TResponse, TDto>(mapper);
    }
}

public static class BaseJsonPatchValidatorExtension
{
    public static IRuleBuilderOptions<TCommand, Operation<TDto>> ValidateOperations
        <TCommand, TResponse, TDto>(
            this IRuleBuilderOptions<TCommand, Operation<TDto>> ruleBuilder,
            IMapper mapper)
            where TCommand : BaseJsonPatchCommand<TResponse, TDto>
            where TResponse : BaseResponse
            where TDto : class, IEditDto, new()
    {
        string canParsePathErrorMessage = null!;
        List<Type> propertyPathTypes = null!;
        ruleBuilder = ruleBuilder
            .Must((c, o) => CanParsePath(o, mapper, out propertyPathTypes, out canParsePathErrorMessage))
            .WithMessage(x => canParsePathErrorMessage)
            .WithErrorCode(JsonPatchValidationErrorCode.CanParsePathValidator.ToString());

        string canParseValueErrorMessage = null!;
        ruleBuilder = ruleBuilder
            .Must((c, o) => CanParseValue(o, mapper, propertyPathTypes, canParsePathErrorMessage != null, out canParseValueErrorMessage))
            .WithMessage(x => canParseValueErrorMessage)
            .WithErrorCode(JsonPatchValidationErrorCode.CanParseValueValidator.ToString());

        ruleBuilder.Custom((o, context) => ValidateValue(o, mapper, context, propertyPathTypes.Last(),
            canParseValueErrorMessage != null || canParsePathErrorMessage != null));

        return ruleBuilder;
    }

    private static bool CanParsePath<TDto>(
        Operation<TDto> operation,
        IMapper mapper,
        out List<Type> propertyTypes,
        out string errorMessage)
        where TDto : class, IEditDto, new()
    {
        var jsonPatchPath = new JsonPatchPath(operation.path);

        bool result = DtoExtension.TryGetSourceJsonPatch<TDto>(
            jsonPatchPath.AsSingleProperty,
            mapper.ConfigurationProvider,
            out propertyTypes,
            out string _,
            out errorMessage);

        if (result)
        {
            switch (operation.OperationType)
            {
                case OperationType.Add:
                    if (propertyTypes.Count > 1 && !(propertyTypes[^2]?.IsCollection() ?? false))
                    {
                        errorMessage = "Operation 'add' is available only for collections.";
                    }
                    else if (int.TryParse(jsonPatchPath.LastSegment, out int _))
                    {
                        errorMessage = "Operation 'add' can not change existing entity.";
                    }
                    break;
                case OperationType.Remove:
                    if (propertyTypes.Count > 1 && !(propertyTypes[^2]?.IsCollection() ?? false))
                    {
                        errorMessage = "Operation 'remove' is available only for collections.";
                    }
                    else if (jsonPatchPath.LastSegment == "-")
                    {
                        errorMessage = "Operation 'remove' can not change not existing entity.";
                    }
                    break;
                default:
                    break;
            }
        }

        if (errorMessage != null)
        {
            errorMessage = $"{operation.path}: {errorMessage}";
            return false;
        }

        return result;
    }

    private static bool CanParseValue<TDto>(
        Operation<TDto> operation,
        IMapper mapper,
        List<Type> propertyTypes,
        bool previousValidationWasThrownWithError,
        out string errorMessage)
        where TDto : class, IEditDto, new()
    {
        errorMessage = null;
        if (propertyTypes == null || operation.OperationType == OperationType.Remove || previousValidationWasThrownWithError)
            return true;

        var jsonPatchPath = new JsonPatchPath(operation.path);

        bool result = DtoExtension.TryGetSourceValueJsonPatch(
                operation.value,
                propertyTypes,
                mapper.ConfigurationProvider,
                out var _,
                out errorMessage,
                jsonPatchPath.LastSegment);
        if (errorMessage != null)
            errorMessage = $"{operation.path}: {errorMessage}";
        return result;
    }

    private static void ValidateValue<TCommand, TDto>(
        Operation<TDto> operation,
        IMapper mapper,
        ValidationContext<TCommand> context,
        Type propertyType,
        bool previousValidationWasThrownWithError)
        where TDto : class, IEditDto, new()
    {
        if (propertyType == null || operation.OperationType == OperationType.Remove || previousValidationWasThrownWithError)
            return;

        switch (operation.OperationType)
        {
            case OperationType.Add:
                ValidateAddition(operation, context, propertyType);
                break;
            case OperationType.Remove:
                break;
            case OperationType.Replace:
                ValidateReplace(operation, mapper, context);
                break;
            default:
                throw new ArgumentException($"Operation {operation.op} is not supported.");
        }
    }

    // TODO: попробовать заменить object на IValidator
    private static void ValidateAddition<TCommand, TDto>(
        Operation<TDto> operation,
        ValidationContext<TCommand> context,
        Type dtoType)
        where TDto : class, IEditDto, new()
    {
        if (!typeof(IEditDto).IsAssignableFrom(dtoType))
            throw new FormatException("Something went wrong while getting type of DTO for 'add' operation");
        object validator = GetValidatorForDto(dtoType, out Type validatorType);
        object dtosList = Activator.CreateInstance(typeof(List<>).MakeGenericType(dtoType));

        JsonPatchPath path = new(operation.path);
        try
        {
            operation.path = '/' + operation.path.Split('/').Last();
            operation.Apply(dtosList, JsonPatchExpressions.Adapter);
        }
        catch (Exception ex)
        {
            return;
        }
        finally
        {
            operation.path = path.OriginalPath;
        }

        MethodInfo validateMethod = validatorType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m =>
                m.Name == nameof(IValidator.Validate) &&
                m.GetParameters().Length == 1)!;
        object[] parameters = [((IList)dtosList)[0]];
        ValidationResult result = (ValidationResult)validateMethod.Invoke(validator, parameters);

        foreach (var error in result.Errors)
        {
            var failure = error;
            failure.PropertyName = context.PropertyName;
            //failure.PropertyName = string.Format("{0}.{1}", context.PropertyName, error.PropertyName);
            context.AddFailure(failure);
        }

    }

    private static void ValidateReplace<TCommand, TDto>(
        Operation<TDto> operation,
        IMapper mapper,
        ValidationContext<TCommand> context)
        where TDto : class, IEditDto, new()
    {
        Type dtoType = GetLastDtoType(operation, mapper);
        if (!typeof(IEditDto).IsAssignableFrom(dtoType))
            throw new FormatException("Something went wrong while getting type of DTO for 'replace' operation");
        object validator = GetValidatorForDto(dtoType, out _);
        object dto = Activator.CreateInstance(dtoType);

        JsonPatchPath path = new(operation.path);
        try
        {
            operation.path = '/' + operation.path.Split('/').Last();
            operation.Apply(dto, JsonPatchExpressions.Adapter);
        }
        catch (Exception ex)
        {
            return;
        }
        finally
        {
            operation.path = path.OriginalPath;
        }

        MethodInfo validateMethod = typeof(DefaultValidatorExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(DefaultValidatorExtensions.Validate));

        var genericValidateMethod = validateMethod.MakeGenericMethod(dtoType);
        var optionsAction = GetValidationOptionsAction(dtoType, path.LastSegment);
        object[] parameters = [validator, dto, optionsAction];
        ValidationResult result = (ValidationResult)genericValidateMethod.Invoke(null, parameters);

        foreach (var error in result.Errors)
        {
            var failure = error;
            failure.PropertyName = context.PropertyName;
            //failure.PropertyName = string.Format("{0}.{1}", context.PropertyName, error.PropertyName);
            context.AddFailure(failure);
        }
    }

    private static object GetValidatorForDto(Type dtoType, out Type validatorType)
    {
        MethodInfo getValidatorMethod = dtoType
            .GetMethod(nameof(IEditDto.GetValidatorType),
                BindingFlags.Static | BindingFlags.Public)!;
        validatorType = (Type)getValidatorMethod.Invoke(null, null);
        object validator = Activator.CreateInstance(validatorType);
        return validator;
    }

    private static Type GetLastDtoType<TDto>(Operation<TDto> operation, IMapper mapper)
        where TDto : class, IEditDto, new()
    {
        string pathWithoutLastSegment = string.Join('/', operation.path.Split('/').SkipLast(1));
        var jsonPatchPath = new JsonPatchPath(pathWithoutLastSegment);
        DtoExtension.GetSourceJsonPatch<TDto>(
            jsonPatchPath.AsSingleProperty,
                mapper.ConfigurationProvider,
                out List<Type> propertyPathTypes);
        return propertyPathTypes.Last();
    }

    private static object GetValidationOptionsAction(Type propertyType, params string[] propertiesToValidate)
    {
        MethodInfo methodInfo = typeof(BaseJsonPatchValidatorExtension)
            .GetMethod(nameof(GetAction), BindingFlags.Static | BindingFlags.NonPublic)!;

        var genericMethod = methodInfo.MakeGenericMethod(propertyType);

        object[] parameters = [propertiesToValidate];

        return genericMethod.Invoke(null, parameters);
    }

    private static Action<ValidationStrategy<T>> GetAction<T>(string[] strings)
    {
        return op => op.IncludeProperties(strings);
    }
}

internal enum JsonPatchValidationErrorCode
{
    CanParsePathValidator,
    CanParseValueValidator,

}
