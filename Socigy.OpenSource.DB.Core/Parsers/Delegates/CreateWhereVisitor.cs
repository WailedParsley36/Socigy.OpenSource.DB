using Socigy.OpenSource.DB.Core.Delegates;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace Socigy.OpenSource.DB.Core.Parsers.Delegates
{
    public delegate ISqlVisitor CreateWhereVisitor(ParameterExpression param, GetColumnName getColName, DbCommand command);
}
