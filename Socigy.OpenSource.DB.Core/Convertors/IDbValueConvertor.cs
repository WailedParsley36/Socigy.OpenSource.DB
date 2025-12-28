using System;
using System.Collections.Generic;
using System.Text;

namespace Socigy.OpenSource.DB.Core.Convertors
{
    public interface IDbValueConvertor<TFrom>
    {
#nullable enable
        object? ConvertToDbValue(TFrom? value);
        TFrom? ConvertFromDbValue(object? dbValue);
#nullable disable
    }
}
