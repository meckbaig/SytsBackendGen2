using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SytsBackendGen2.Application.Common.Dtos;

namespace SytsBackendGen2.Application.Common.BaseRequests.AuthentificatedRequest
{
    public abstract record BaseAuthentificatedRequest<TResponse> : BaseRequest<TResponse> where TResponse : BaseResponse
    {
        virtual internal bool loggedIn { get; set; } = false;
        abstract internal int userId { get; set; }

        public virtual void SetUserId(int? userId)
        {
            this.userId = userId ?? 0;
            loggedIn = userId > 0 ? true : false;
        }
    }
}
