using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Application.Common.BaseRequests;
using SytsBackendGen2.Application.Common.BaseRequests.JsonPatchCommand;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.DTOs.Roles;
using SytsBackendGen2.Application.Extensions.JsonPatch;

namespace SytsBackendGen2.Application.Services.Roles;

public record JsonPatchRolesCommand : BaseJsonPatchCommand<JsonPatchRolesResponse, RoleEditDto>
{
    public override JsonPatchDocument<RoleEditDto> Patch { get; set; }
}

public class JsonPatchRolesResponse : BaseResponse
{
    public List<RolePreviewDto> Roles { get; set; }
}

public class JsonPatchRolesCommandValidator : BaseJsonPatchValidator
    <JsonPatchRolesCommand, JsonPatchRolesResponse, RoleEditDto>
{
    public JsonPatchRolesCommandValidator(IMapper mapper) : base(mapper)
    {
    }
}

public class JsonPatchRolesCommandHandler : IRequestHandler<JsonPatchRolesCommand, JsonPatchRolesResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public JsonPatchRolesCommandHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<JsonPatchRolesResponse> Handle(JsonPatchRolesCommand request, CancellationToken cancellationToken)
    {
        request.Patch.ApplyDtoTransactionToSource(_context.Roles, _mapper.ConfigurationProvider);

        var roles = _context.Roles.AsNoTracking()
            .Include(r => r.Permissions)
            .Select(r => _mapper.Map<RolePreviewDto>(r))
            .ToList();

        return new JsonPatchRolesResponse { Roles = roles };
    }
}
