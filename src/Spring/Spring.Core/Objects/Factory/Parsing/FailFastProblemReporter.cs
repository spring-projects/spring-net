using System;
using System.Collections.Generic;
using System.Text;
using Common.Logging;

namespace Spring.Objects.Factory.Parsing
{
    public class FailFastProblemReporter : IProblemReporter
    {
        private ILog _logger = LogManager.GetLogger(typeof(FailFastProblemReporter));

        public ILog Logger
        {
            get { return _logger; }
        }

        public void Error(Problem problem)
        {
            _logger.Error(problem.Message);
            throw new ObjectDefinitionParsingException(problem);
        }

        public void Fatal(Problem problem)
        {
            _logger.Fatal(problem.Message);
            throw new ObjectDefinitionParsingException(problem);
        }

        public void Warning(Problem problem)
        {
            _logger.Warn(problem.Message);
         
        }

    }
}
