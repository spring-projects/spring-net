using System;
using System.Collections.Generic;
using System.Text;

namespace Spring.Objects.Factory.Parsing
{
    public interface IProblemReporter
    {
        void Fatal(Problem problem);
        void Warning(Problem problem);
        void Error(Problem problem);
    }
}
