using System;
using System.Collections.Generic;
using System.Text;

namespace Spring.Context.Attributes
{
    /// <summary>
    /// Indicates that an object should be given preference when multiple candidates
    /// are qualified to autowire a single-valued dependency. If exactly one 'primary'
    /// object exists among the candidates, it will be the autowired value.
    /// 
    /// <para>Using <see cref="Primary"/> at the class level has no effect unless component-scanning
    /// is being used. If a <see cref="Primary"/>-attributed class is declared via XML,
    /// <see cref="Primary"/> attribute metadata is ignored, and
    /// <code>&lt;object primary="true|false"/&gt;></code> is respected instead.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PrimaryAttribute : Attribute
    {

    }
}
