/*
 * Copyright 2002-2010 the original author or authors.
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

using Spring.Transaction.Support;

namespace Spring.Transaction;

/// <summary>
/// This is a test implementation to count the number of times PlatformTransactionManager methods
/// have been called.
/// </summary>
/// <author>Mark Pollack</author>
public class CallCountingTransactionManager : AbstractPlatformTransactionManager
{
    public int begun;
    public int commits;
    public int rollbacks;
    public int inflight;

    protected override object DoGetTransaction()
    {
        return new object();
    }

    protected override void DoBegin(object transaction, ITransactionDefinition definition)
    {
        ++begun;
        ++inflight;
    }

    protected override void DoCommit(DefaultTransactionStatus status)
    {
        ++commits;
        --inflight;
    }

    protected override void DoRollback(DefaultTransactionStatus status)
    {
        ++rollbacks;
        --inflight;
    }

    public void Clear()
    {
        begun = commits = rollbacks = inflight = 0;
    }
}
