using System.Collections;
using System.Linq.Expressions;
using System.Runtime.InteropServices.JavaScript;
using SytsBackendGen2.Application.Extensions.ListFilters;

namespace SytsBackendGen2.Application.Common.BaseRequests.ListQuery;

public abstract record BaseListQuery<TResponse> : BaseRequest<TResponse>
    where TResponse : BaseResponse
{
    // ReSharper disable InconsistentNaming
    public virtual int skip { get; set; }
    public virtual int take { get; set; }
    public virtual string[]? filters { get; set; }
    public virtual string[]? orderBy { get; set; }
    // ReSharper restore InconsistentNaming

    private readonly List<Expression> _filterExpressions = [];
    private readonly List<Expression> _orderExpressions = [];
    
    public List<Expression> GetFilterExpressions() 
        => _filterExpressions;
    
    public List<Expression> GetOrderExpressions() 
        => _orderExpressions;

    public void AddFilterExpression(Expression expression)
        => _filterExpressions!.Add(expression);

    public void AddOrderExpression(Expression expression)
        => _orderExpressions!.Add(expression);
}