using AutoMapper;
using FluentValidation;
using MediatR;
using ProjectName.Application.Common.BaseRequests;
using ProjectName.Application.Common.BaseRequests.JsonPatchCommand;
using ProjectName.Application.Common.Interfaces;

namespace ProjectName.Application.Services.FeatureName;

public record JsonPatchServiceMethodCommand : BaseJsonPatchCommand<JsonPatchServiceMethodResponse, IEditDto>
{

}

public class JsonPatchServiceMethodResponse : BaseResponse
{
	
}

public class JsonPatchServiceMethodCommandValidator : BaseJsonPatchValidator
    <JsonPatchServiceMethodCommand, JsonPatchServiceMethodResponse, IEditDto>
{
    public JsonPatchServiceMethodCommandValidator(IMapper mapper) : base(mapper) { }
}

public class JsonPatchServiceMethodCommandHandler : IRequestHandler<JsonPatchServiceMethodCommand, JsonPatchServiceMethodResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public JsonPatchServiceMethodCommandHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<JsonPatchServiceMethodResponse> Handle(JsonPatchServiceMethodCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
