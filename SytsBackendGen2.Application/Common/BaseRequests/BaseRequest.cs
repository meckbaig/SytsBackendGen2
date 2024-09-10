using System.Collections;
using AutoMapper.Internal;
using MediatR;

namespace SytsBackendGen2.Application.Common.BaseRequests;

public record BaseRequest<TResponse> : IRequest<TResponse> where TResponse : BaseResponse
{
    private string? _key = null;
    public virtual string GetKey()
    {
        if (_key is null)
        {
            Dictionary<string, string> props = new();
            foreach (var prop in GetType().GetProperties())
            {
                var value = prop.GetValue(this, null);
                if (value != null)
                {
                    if (value.GetType().IsCollection())
                    {
                        string collection = string.Join(',', ((object[])value).Select(x => x.ToString()));
                        props.Add(prop.Name, collection);
                    }
                    else
                    {
                        props.Add(prop.Name, value.ToString()!);
                    }
                }
            }
            _key = $"{GetType().Name}-{string.Join(';', props)}";
        }
        return _key;
    }
}