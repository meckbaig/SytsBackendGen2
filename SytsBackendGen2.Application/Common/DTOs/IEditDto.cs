using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.Common.Dtos;

public interface IEditDto : IBaseDto
{
    static abstract Type GetValidatorType();
}
