using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using SytsBackendGen2.Application.Common.BaseRequests.JsonPatchCommand;
using SytsBackendGen2.Application.UnitTests.Common.DTOs;
using SytsBackendGen2.Application.UnitTests.Common.Mediators;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace SytsBackendGen2.Application.UnitTests.JsonPatch;

public class JsonPatchValidationTests
{
    private readonly IMapper _mapper;
    private readonly Newtonsoft.Json.JsonSerializer _jsonSerializer;

    public JsonPatchValidationTests()
    {
        var config = new MapperConfiguration(c =>
        {
            c.AddProfile<TestEntityDto.Mapping>();
            c.AddProfile<TestNestedEntityDto.Mapping>();
            c.AddProfile<TestEntityEditDto.Mapping>();
            c.AddProfile<TestNestedEntityEditDto.Mapping>();
            c.AddProfile<TestEditDtoWithLongNameMapping.Mapping>();
        });
        _mapper = config.CreateMapper();
        _jsonSerializer = new Newtonsoft.Json.JsonSerializer() { ContractResolver = new CamelCasePropertyNamesContractResolver() };
    }

    #region Success

    [Fact]
    public async Task Validate_ReturnsOk_WhenDefault()
    {
        // Arrange
        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>()
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public async Task ValidateReplace_ReturnsOk_WhenStringProperty()
    {
        // Arrange
        string newEntityName = "NewValue1";

        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "replace",
                path = "/1/entityName",
                value = newEntityName
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public async Task ValidateReplace_ReturnsOk_WhenNestedStringPropertyInModel()
    {
        // Arrange
        string newEntityName = "NewValue1";

        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "replace",
                path = "/1/someInnerEntity/nestedName",
                value = newEntityName
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public async Task ValidateReplace_ReturnsOk_WhenNestedStringPropertyInCollection()
    {
        // Arrange
        string newEntityName = "NewValue1";

        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "replace",
                path = "/1/nestedThings/2/nestedName",
                value = newEntityName
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public async Task ValidateReplace_ReturnsOk_WhenPropertyWithComplexMapping()
    {
        // Arrange
        string newDate = "31 декабря 2020 г.";

        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "replace",
                path = "/1/dateString",
                value = newDate
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public async Task ValidateReplace_ReturnsOk_WhenPropertyWithModelId()
    {
        // Arrange
        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "replace",
                path = "/1/someInnerEntityId",
                value = 2
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public async Task ValidateReplace_ReturnsOk_WhenPropertyWithLongMapping()
    {
        // Arrange
        string newName = "NewNestedEntityNameValue1";

        List<Operation<TestEditDtoWithLongNameMapping>> operations = new()
        {
            new Operation<TestEditDtoWithLongNameMapping>
            {
                op = "replace",
                path = "/1/nestedName",
                value = newName
            }
        };

        var command = new TestJsonPatchLongMappingCommand
        {
            Patch = new JsonPatchDocument<TestEditDtoWithLongNameMapping>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchLongMappingCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public async Task ValidateAdd_ReturnsOk_WhenModelIntoCollectionWithManyToMany()
    {
        // Arrange
        var newModel = new TestNestedEntityEditDto
        {
            Id = 4,
            NestedName = "qqqqqqqqqqqq",
            Number = 111111
        };
        JObject jsonModel = JObject.FromObject(newModel, _jsonSerializer);

        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "add",
                path = "/1/nestedThings/-",
                value = jsonModel
            }
        };


        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public async Task ValidateAdd_ReturnsOk_WhenModelIntoDB()
    {
        // Arrange
        var nestedThings = new List<TestNestedEntityEditDto>
        {
            new () { Id = 4, NestedName = "12343567", Number = 12344 },
            new () { Id = 6, NestedName = "12343567", Number = 12344  },
            new () { Id = 8, NestedName = "12343567", Number = 12344  }
        };

        var newModel = new TestEntityEditDto
        {
            Id = 11,
            EntityName = "NewAddedTestEntity For ValidateAdd_ReturnsOk_WhenModelIntoDB",
            OriginalDescription = "New Description",
            DateString = new DateOnly(2022, 1, 2).ToLongDateString(),
            SomeInnerEntityId = 10,
            NestedThings = nestedThings
        };
        JObject jsonModel = JObject.FromObject(newModel, _jsonSerializer);

        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "add",
                path = "/-",
                value = jsonModel
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public async Task ValidateRemove_ReturnsOk_WhenRemoveNestedEntityWithManyToMany()
    {
        // Arrange
        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "remove",
                path = "/2/nestedThings/2"
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public async Task ValidateRemove_ReturnsOk_WhenModelFromDB()
    {
        // Arrange
        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "remove",
                path = "/3"
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
    }

    #endregion

    #region Error

    [Fact]
    public async Task ValidateReplace_ReturnsValidationError_WhenWrongPropertyName()
    {
        // Arrange
        string newEntityName = "NewValue1";

        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "replace",
                path = "/1/name",
                value = newEntityName
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.False(validationResult.IsValid);
        Assert.Equal(
            JsonPatchValidationErrorCode.CanParsePathValidator.ToString(),
            validationResult.Errors[0].ErrorCode);
        Assert.Equal(
            "/1/name: Property 'name' does not exist",
            validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ValidateReplace_ReturnsValidationError_WhenWrongPropertyType()
    {
        // Arrange
        string newEntityName = "NewValue1";

        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "replace",
                path = "/1/name",
                value = newEntityName
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.False(validationResult.IsValid);
        Assert.Equal(
            JsonPatchValidationErrorCode.CanParsePathValidator.ToString(),
            validationResult.Errors[0].ErrorCode);
        Assert.Equal(
            "/1/name: Property 'name' does not exist",
            validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ValidateReplace_ReturnsValidationError_WhenWrongPropertyValue()
    {
        // Arrange
        string newEntityName = "";

        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "replace",
                path = "/1/nestedThings/1/nestedName",
                value = newEntityName
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.False(validationResult.IsValid);
        Assert.Equal(
            "NotEmptyValidator",
            validationResult.Errors[0].ErrorCode);
        Assert.Equal(
            "'Nested Name' must not be empty.",
            validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ValidateAdd_ReturnsValidationError_WhenPropertyIsNotCollection()
    {
        // Arrange
        string newEntityName = "New name 1";

        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "add",
                path = "/1/nestedThings/1/nestedName",
                value = newEntityName
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.False(validationResult.IsValid);
        Assert.Equal(
            JsonPatchValidationErrorCode.CanParsePathValidator.ToString(),
            validationResult.Errors[0].ErrorCode);
        Assert.Equal(
            "/1/nestedThings/1/nestedName: Operation 'add' is available only for collections.",
            validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ValidateAdd_ReturnsValidationError_WhenWrongPropertyType()
    {
        // Arrange
        string newEntityName = "New name 1";

        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "add",
                path = "/1/nestedThings/-",
                value = newEntityName
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.False(validationResult.IsValid);
        Assert.Equal(
            JsonPatchValidationErrorCode.CanParseValueValidator.ToString(),
            validationResult.Errors[0].ErrorCode);
        Assert.Equal(
            "/1/nestedThings/-: Value is not valid.",
            validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ValidateAdd_ReturnsValidationError_WhenNumberAtTheEnd()
    {
        // Arrange
        object newEntityName = null;

        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "add",
                path = "/1/nestedThings/2",
                value = newEntityName
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.False(validationResult.IsValid);
        Assert.Equal(
            JsonPatchValidationErrorCode.CanParsePathValidator.ToString(),
            validationResult.Errors[0].ErrorCode);
        Assert.Equal(
            "/1/nestedThings/2: Operation 'add' can not change existing entity.",
            validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ValidateRemove_ReturnsValidationError_WhenPropertyIsNotCollection()
    {
        // Arrange
        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "remove",
                path = "/1/nestedThings/1/nestedName"
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.False(validationResult.IsValid);
        Assert.Equal(
            JsonPatchValidationErrorCode.CanParsePathValidator.ToString(),
            validationResult.Errors[0].ErrorCode);
        Assert.Equal(
            "/1/nestedThings/1/nestedName: Operation 'remove' is available only for collections.",
            validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ValidateRemove_ReturnsValidationError_WhenNotANumberAtTheEnd()
    {
        // Arrange
        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "remove",
                path = "/1/nestedThings/-",
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.False(validationResult.IsValid);
        Assert.Equal(
            JsonPatchValidationErrorCode.CanParsePathValidator.ToString(),
            validationResult.Errors[0].ErrorCode);
        Assert.Equal(
            "/1/nestedThings/-: Operation 'remove' can not change not existing entity.",
            validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ValidateRemove_ReturnsValidationError_WhenNotANumberAtTheEndFromDb()
    {
        // Arrange
        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "remove",
                path = "/-",
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.False(validationResult.IsValid);
        Assert.Equal(
            JsonPatchValidationErrorCode.CanParsePathValidator.ToString(),
            validationResult.Errors[0].ErrorCode);
        Assert.Equal(
            "/-: Operation 'remove' can not change not existing entity.",
            validationResult.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task ValidateRemove_ReturnsValidationError_WhenMultipleDashes()
    {
        // Arrange
        List<Operation<TestEntityEditDto>> operations = new()
        {
            new Operation<TestEntityEditDto>
            {
                op = "remove",
                path = "/1/-/-",
            }
        };

        var command = new TestJsonPatchCommand
        {
            Patch = new JsonPatchDocument<TestEntityEditDto>(
                operations,
                new CamelCasePropertyNamesContractResolver())
        };

        var validator = new TestJsonPatchCommandValidator(_mapper);

        // Act
        var validationResult = validator.Validate(command);

        // Assert
        Assert.NotNull(validationResult);
        Assert.False(validationResult.IsValid);
        Assert.Equal(
            JsonPatchValidationErrorCode.CanParsePathValidator.ToString(),
            validationResult.Errors[0].ErrorCode);
        Assert.Equal(
            "/1/-/-: Property '-' does not exist",
            validationResult.Errors[0].ErrorMessage);
    }

    #endregion
}