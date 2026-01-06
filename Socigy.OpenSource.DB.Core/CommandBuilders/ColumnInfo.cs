using System;
using System.Collections.Generic;
using System.Text;

namespace Socigy.OpenSource.DB.Core.CommandBuilders
{
#nullable enable
    public struct ColumnInfo
    {
        public Type Type { get; set; }
        public object? Value { get; set; }
    }
#nullable disable
}
