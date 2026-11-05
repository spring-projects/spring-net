/*
 * Copyright 2002-2026 the original author or authors.
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

using Quartz;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// The process-wide table of the schedulers that <see cref="SchedulerFactoryObject"/> instances expose
/// (<see cref="SchedulerFactoryObject.ExposeSchedulerInRepository"/>).
/// </summary>
/// <remarks>
/// Quartz 3 kept these in a static <c>SchedulerRepository.Instance</c>. Quartz 4 gives each scheduler builder a repository
/// of its own, and disposing the factory that owns a repository shuts down every scheduler in it, so Spring keeps this
/// table instead of sharing one Quartz repository between factories.
/// </remarks>
internal static class SchedulerRegistry
{
    private static readonly Dictionary<string, IScheduler> s_schedulers = new(StringComparer.Ordinal);

    /// <summary>
    /// The lock that a caller holds while it checks for a name, builds a scheduler and binds it, so that two factories
    /// asking for the same name cannot both succeed.
    /// </summary>
    public static object SyncRoot { get; } = new();

    public static IScheduler Lookup(string name)
    {
        lock (SyncRoot)
        {
            return s_schedulers.TryGetValue(name, out IScheduler scheduler) ? scheduler : null;
        }
    }

    /// <summary>
    /// Binds the scheduler under its name.
    /// </summary>
    /// <exception cref="InvalidOperationException">if a scheduler with that name is already bound.</exception>
    public static void Bind(IScheduler scheduler)
    {
        lock (SyncRoot)
        {
            if (!s_schedulers.TryAdd(scheduler.SchedulerName, scheduler))
            {
                throw AlreadyRegistered(scheduler.SchedulerName);
            }
        }
    }

    /// <summary>
    /// Removes the scheduler, unless the name has been taken over by another one.
    /// </summary>
    public static void Remove(IScheduler scheduler)
    {
        lock (SyncRoot)
        {
            if (s_schedulers.TryGetValue(scheduler.SchedulerName, out IScheduler bound) && ReferenceEquals(bound, scheduler))
            {
                s_schedulers.Remove(scheduler.SchedulerName);
            }
        }
    }

    public static void AssertNotRegistered(string name)
    {
        lock (SyncRoot)
        {
            if (s_schedulers.ContainsKey(name))
            {
                throw AlreadyRegistered(name);
            }
        }
    }

    private static InvalidOperationException AlreadyRegistered(string name)
    {
        return new InvalidOperationException(
            $"Active Scheduler of name '{name}' already registered in Quartz SchedulerRepository. Cannot create a new Spring-managed Scheduler of the same name!");
    }
}
