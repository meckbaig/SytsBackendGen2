using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Application.Common.BaseRequests;
using SytsBackendGen2.Application.Common.BaseRequests.JsonPatchCommand;
using SytsBackendGen2.Application.Extensions.JsonPatch;
using SytsBackendGen2.Application.UnitTests.Common.DTOs;

namespace SytsBackendGen2.Application.UnitTests.Common.Mediators;

public record TestJsonPatchCommand : BaseJsonPatchCommand<TestJsonPatchResponse, TestEntityEditDto>
{
    public override JsonPatchDocument<TestEntityEditDto> Patch { get; set; }
}

public class TestJsonPatchResponse : BaseResponse
{
    public List<TestEntityDto> TestEntities { get; set; }
}

public class TestJsonPatchCommandValidator : BaseJsonPatchValidator
    <TestJsonPatchCommand, TestJsonPatchResponse, TestEntityEditDto>
{
    public TestJsonPatchCommandValidator(IMapper mapper) : base(mapper) { }
}

public class TestJsonPatchCommandHandler : IRequestHandler<TestJsonPatchCommand, TestJsonPatchResponse>
{
    private readonly TestDbContext _context;
    private readonly IMapper _mapper;

    public TestJsonPatchCommandHandler(TestDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<TestJsonPatchResponse> Handle(TestJsonPatchCommand request, CancellationToken cancellationToken)
    {
        request.Patch.ApplyDtoTransactionToSource(_context.TestEntities, _mapper.ConfigurationProvider);

        var entities = _context.TestEntities
            .Include(e => e.InnerEntity)
            .Include(e => e.TestNestedEntities)
            .AsNoTracking()
            .ProjectTo<TestEntityDto>(_mapper.ConfigurationProvider)
            .ToList();

        return new TestJsonPatchResponse { TestEntities = entities };
    }
}