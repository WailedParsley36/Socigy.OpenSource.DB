using System;
using System.Collections.Generic;
using System.Text;

namespace Socigy.OpenSource.DB.Core.Enums
{
    [Flags]
    public enum JoinType
    {
        // 0 is usually reserved for None/Default
        None = 0,

        // --- Base Types ---
        Inner = 1 << 0, // 1
        Cross = 1 << 1, // 2

        // --- Directionals (Implicitly Outer) ---
        Left = 1 << 2, // 4
        Right = 1 << 3, // 8

        // Composite: Full is semantically both Left AND Right
        Full = Left | Right, // 12

        // --- Modifiers ---
        // "NATURAL" changes how columns are matched
        Natural = 1 << 4, // 16

        // "OUTER" is often optional syntax (LEFT JOIN vs LEFT OUTER JOIN),
        // but this flag allows you to be explicit if your SQL dialect requires it.
        ExplicitOuter = 1 << 5  // 32
    }
}
