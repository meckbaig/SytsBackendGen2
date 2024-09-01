using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.Extensions.JsonPatch;

public class DbSetOperation<TModel> : Operation<DbSet<TModel>>, IDbSetOperation where TModel : BaseEntity
{
    public string dtoPath { get; set; }
}

public class ReguldarOperation : Operation
{
    public override string ToString() => path;
}

public interface IDbSetOperation
{
    string dtoPath { get; set; }
}