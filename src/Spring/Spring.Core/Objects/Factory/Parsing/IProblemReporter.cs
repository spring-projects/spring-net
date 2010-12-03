#if NET_2_0

using System;
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

#endif