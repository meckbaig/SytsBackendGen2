using AutoMapper;
using SytsBackendGen2.Application.Common;
using SytsBackendGen2.Application.Extensions.ListFilters;
using SytsBackendGen2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.Extensions.ListFilters
{
    public interface IEntityFrameworkExpression<T> where T : Enum
    {
        public string? Key { get; set; }
        public string? EndPoint { get; set; }
        public T ExpressionType { get; set; }
    }  
}
