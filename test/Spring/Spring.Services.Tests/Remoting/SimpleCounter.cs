/*
 * Copyright 2004 the original author or authors.
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

namespace Spring.Remoting;

/// <summary>
/// A simple remote class.
/// </summary>
public class SimpleCounter : ISimpleCounter
{
    protected int _counter;

    /// <summary>
    /// Create an instance of the Counter class.
    /// </summary>
    public SimpleCounter()
    {
        _counter = 0;
    }

    /// <summary>
    /// Create an instance of the Counter class.
    /// </summary>
    /// <param name="initialCounter">Initial counter value.</param>
    public SimpleCounter(int initialCounter)
    {
        _counter = initialCounter;
    }

    /// <summary>
    /// Get or Set the Counter's value.
    /// </summary>
    public int Counter
    {
        get { return _counter; }
        set { _counter = value; }
    }

    /// <summary>
    /// Increment the counter by one.
    /// </summary>
    public void Count()
    {
        _counter++;
    }
}
