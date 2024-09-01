using Microsoft.AspNetCore.JsonPatch;
using SytsBackendGen2.Application.Common.Dtos;

namespace SytsBackendGen2.Application.Common.BaseRequests.JsonPatchCommand;

public abstract record BaseJsonPatchCommand<TResponse, TDto> : BaseRequest<TResponse>
    where TResponse : BaseResponse
    where TDto : class, IEditDto, new()
{
    public abstract JsonPatchDocument<TDto> Patch { get; set; }
}
