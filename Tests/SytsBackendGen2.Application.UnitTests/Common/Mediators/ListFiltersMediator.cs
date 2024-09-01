using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using SytsBackendGen2.Application.Common.BaseRequests.ListQuery;
using SytsBackendGen2.Application.Extensions.ListFilters;
using SytsBackendGen2.Application.UnitTests.Common.DTOs;
using SytsBackendGen2.Application.UnitTests.Common.Entities;
using static SytsBackendGen2.Application.UnitTests.Common.ValidationTestsEntites;

namespace SytsBackendGen2.Application.UnitTests.Common.Mediators;


public record TestQuery : BaseListQuery<TestResponse>
{

}

public class TestResponse : BaseListQueryResponse<TestEntityDto>
{

}

public class TestQueryValidator : BaseListQueryValidator
    <TestQuery, TestResponse, TestEntityDto, TestEntity>
{
    public TestQueryValidator(IMapper mapper) : base(mapper)
    {

    }
}

public class TestQueryHandler : IRequestHandler<TestQuery, TestResponse>
{
    private readonly IMapper _mapper;

    public TestQueryHandler(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task<TestResponse> Handle(
        TestQuery request,
        CancellationToken cancellationToken)
    {
        int total = request.skip + (request.take != 0 ? request.take : 10);
        var result = SeedTestEntities(total)
            .AddFilters(request.GetFilterExpressions())
            .AddOrderBy(request.GetOrderExpressions())
            .Skip(request.skip).Take(request.take > 0 ? request.take : int.MaxValue)
            .ProjectTo<TestEntityDto>(_mapper.ConfigurationProvider)
            .ToList();
        return new TestResponse { Items = result };
    }
}
