using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SytsBackendGen2.Infrastructure.Caching.AsyncSqlCachedKeysProvider;

namespace SytsBackendGen2.Infrastructure.AsyncMessaging;

internal class CachedKeyDataMessage
{
    public CachedKeyData CachedKey { get; set; }
}
