using AutoMapper;
using SytsBackendGen2.Application.UnitTests.Common.DTOs;
using SytsBackendGen2.Application.UnitTests.Common.Mediators;
using Xunit;

namespace SytsBackendGen2.Application.UnitTests.ListFilters;

public class ListFiltersImplementationTests
{
    private readonly IMapper _mapper;

    public ListFiltersImplementationTests()
    {
        var config = new MapperConfiguration(c =>
        {
            c.AddProfile<TestEntityDto.Mapping>();
            c.AddProfile<TestNestedEntityDto.Mapping>();
        });
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task TestDtoValue_ReturnsListOfSingleStartingWithId10_WhenSkip10Take1()
    {
        // Arrange
        var query = new TestQuery { skip = 10, take = 1 };

        var validator = new TestQueryValidator(_mapper);
        var handler = new TestQueryHandler(_mapper);

        var referenceDto = new TestEntityDto
        {
            Id = 10,
            EntityName = "Name10",
            OriginalDescription = "Description10",
            ReverseDescription = "01noitpircseD",
            SomeInnerEntity = new() { Id = 110, NestedName = "NestedName110", Number = 165 },
            DateString = "22 декабря 2023 г.",
            SomeCount = 0,
            NestedThings = new List<TestNestedEntityDto>
            {
                new() {
                    Id = 10,
                    NestedName = $"NestedName10",
                    Number = 15
                },
                new() {
                    Id = 11,
                    NestedName = $"NestedName11",
                    Number = 16
                },
                new() {
                    Id = 12,
                    NestedName = $"NestedName12",
                    Number = 18
                }
            }
        };

        // Act
        var validationResult = validator.Validate(query);
        TestResponse result = null;
        if (validationResult.IsValid)
            result = await handler.Handle(query, default);

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.NotNull(result);
        foreach (var prop in typeof(TestEntityDto).GetProperties())
        {
            object v1 = prop.GetValue(referenceDto);
            object v2 = prop.GetValue(result.Items[0]);
            Assert.Equal(v1, v2);
        }
    }

    [Fact]
    public async Task TestFilters_ReturnsListOf1WithCorrespondingId_WhenFilterBySingleId()
    {
        // Arrange
        var query = new TestQuery { filters = ["id:3"] };

        var validator = new TestQueryValidator(_mapper);
        var handler = new TestQueryHandler(_mapper);

        // Act
        var validationResult = validator.Validate(query);
        TestResponse result = null;
        if (validationResult.IsValid)
            result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
        Assert.NotNull(result?.Items?[0]);
        Assert.Equal(3, result?.Items?[0].Id);
    }

    [Fact]
    public async Task TestFilters_ReturnsListWithCorrespondingId_WhenFilterByRangeOfIds()
    {
        // Arrange
        var query = new TestQuery { filters = ["id:3..7"] };

        var validator = new TestQueryValidator(_mapper);
        var handler = new TestQueryHandler(_mapper);

        // Act
        var validationResult = validator.Validate(query);
        TestResponse result = null;
        if (validationResult.IsValid)
            result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
        Assert.NotNull(result?.Items);
        Assert.DoesNotContain(result.Items, x => x.Id < 3 || x.Id > 7);
    }

    [Fact]
    public async Task TestFilters_ReturnsListOf0_WhenFilterOutOfRange()
    {
        // Arrange
        var query = new TestQuery { filters = ["id:133"] };

        var validator = new TestQueryValidator(_mapper);
        var handler = new TestQueryHandler(_mapper);

        // Act
        var validationResult = validator.Validate(query);
        TestResponse result = null;
        if (validationResult.IsValid)
            result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
        Assert.NotNull(result?.Items);
        Assert.Equal(0, result.Items.Count());
    }

    [Fact]
    public async Task TestFilters_ReturnsListOfCorrespondingId_WhenFilterWithoutUpperLimit()
    {
        // Arrange
        var query = new TestQuery { filters = ["id:3.."] };

        var validator = new TestQueryValidator(_mapper);
        var handler = new TestQueryHandler(_mapper);

        // Act
        var validationResult = validator.Validate(query);
        TestResponse result = null;
        if (validationResult.IsValid)
            result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
        Assert.NotNull(result?.Items);
        Assert.DoesNotContain(result.Items, x => x.Id < 3);
    }

    [Fact]
    public async Task TestFilters_ReturnsListOfCorrespondingId_WhenFilterWithoutBottomLimit()
    {
        // Arrange
        var query = new TestQuery { filters = ["id:..7"] };

        var validator = new TestQueryValidator(_mapper);
        var handler = new TestQueryHandler(_mapper);

        // Act
        var validationResult = validator.Validate(query);
        TestResponse result = null;
        if (validationResult.IsValid)
            result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
        Assert.NotNull(result?.Items);
        Assert.DoesNotContain(result.Items, x => x.Id > 7);
    }

    [Fact]
    public async Task TestFilters_ReturnsList_WhenFilterSingleEntityById()
    {
        // Arrange
        var query = new TestQuery { filters = ["someInnerEntity:107"] };

        var validator = new TestQueryValidator(_mapper);
        var handler = new TestQueryHandler(_mapper);

        // Act
        var validationResult = validator.Validate(query);
        TestResponse result = null;
        if (validationResult.IsValid)
            result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
        Assert.NotNull(result?.Items);
        Assert.Equal(107, result.Items?[0]?.SomeInnerEntity?.Id);
    }

    [Fact]
    public async Task TestFilters_ReturnsList_WhenFilterByRangeOfEntitiesById()
    {
        // Arrange
        var query = new TestQuery { filters = ["someInnerEntity:105..107"] };

        var validator = new TestQueryValidator(_mapper);
        var handler = new TestQueryHandler(_mapper);

        // Act
        var validationResult = validator.Validate(query);
        TestResponse result = null;
        if (validationResult.IsValid)
            result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
        Assert.NotNull(result?.Items);
        Assert.False(result.Items.Any(x => x.SomeInnerEntity?.Id < 105 || x.SomeInnerEntity?.Id > 107));
    }

    [Fact]
    public async Task TestFilters_ReturnsListWithAtLeastOneNestedPropertyWithId5_WhenFilterByNestedPeopertyWithId5()
    {
        // Arrange
        var query = new TestQuery { filters = ["nestedThings.id:5"] };

        var validator = new TestQueryValidator(_mapper);
        var handler = new TestQueryHandler(_mapper);

        // Act
        var validationResult = validator.Validate(query);
        TestResponse result = null;
        if (validationResult.IsValid)
            result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
        Assert.NotNull(result?.Items);
        // i, i+1, i+2 => 5, 4+1, 3+1
        Assert.Equal(3, result.Items.Count());
    }

    [Fact]
    public async Task TestOrderBy_ReturnsListOrderedByIdDesc_WhenOrderByIdDesc()
    {
        // Arrange
        var query = new TestQuery { orderBy = ["id desc"] };

        var validator = new TestQueryValidator(_mapper);
        var handler = new TestQueryHandler(_mapper);

        // Act
        var validationResult = validator.Validate(query);
        TestResponse result = null;
        if (validationResult.IsValid)
            result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
        Assert.NotNull(result?.Items);
        Assert.Equal(result.Items.OrderByDescending(x => x.Id), result.Items);
    }

    [Fact]
    public async Task TestOrderBy_ReturnsListOrderedByCount_WhenOrderByCount()
    {
        // Arrange
        var query = new TestQuery { orderBy = ["someCount"] };

        var validator = new TestQueryValidator(_mapper);
        var handler = new TestQueryHandler(_mapper);

        // Act
        var validationResult = validator.Validate(query);
        TestResponse result = null;
        if (validationResult.IsValid)
            result = await handler.Handle(query, default);

        // Assert
        Assert.NotNull(validationResult);
        Assert.True(validationResult.IsValid);
        Assert.NotNull(result?.Items);
        Assert.Equal(result.Items.OrderBy(x => x.SomeCount), result.Items);
    }

}
