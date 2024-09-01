using SytsBackendGen2.Application.Common.Dtos;

namespace SytsBackendGen2.Application.Common.BaseRequests.ListQuery
{
    public abstract class BaseListQueryResponse<TResult> : BaseResponse where TResult : IBaseDto
    {
        public virtual IList<TResult> Items { get; set; }
    }
}