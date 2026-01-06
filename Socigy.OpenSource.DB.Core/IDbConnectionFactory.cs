using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace Socigy.OpenSource.DB.Core
{
#nullable enable
    public interface IDbConnectionFactory
    {
        DbConnection Create(string? connectionKey = null);
        Task<bool> EnsureDbExists();
    }
#nullable disable
}
