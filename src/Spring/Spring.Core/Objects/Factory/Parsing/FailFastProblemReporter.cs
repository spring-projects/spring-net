#region License

/*
 * Copyright © 2002-2011 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

using Common.Logging;

namespace Spring.Objects.Factory.Parsing
{
    public class FailFastProblemReporter : IProblemReporter
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FailFastProblemReporter));

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
