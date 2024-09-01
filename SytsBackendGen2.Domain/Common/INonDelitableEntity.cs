using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Domain.Common;

public interface INonDelitableEntity
{
    public bool Deleted { get; set; }
}
