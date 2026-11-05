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

using System.Text.RegularExpressions;
using NUnit.Framework;
using Quartz;
using Spring.Context.Support;
using Spring.Core.IO;

namespace Spring.Scheduling.Quartz;

/// <summary>
/// The XML snippet in the README of the package loads against the code, and does what the README says.
/// </summary>
[TestFixture]
public class ReadmeSnippetTests
{
    [Test]
    public void TestUsageSnippetLoadsAndSchedulesTheCall()
    {
        string readme = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "README.md"));
        Match snippet = Regex.Match(readme, "```xml\\s*(?<xml>.*?)```", RegexOptions.Singleline);
        Assert.That(snippet.Success, Is.True, "the README has no XML snippet");

        // The snippet names a class of the application; the test has its own.
        string xml = snippet.Groups["xml"].Value
            .Replace("MyApp.ExportService, MyApp", "Spring.Scheduling.Quartz.QuartzTestObject, Spring.Scheduling.Quartz4.Tests");
        Assert.That(xml, Does.Contain("Spring.Scheduling.Quartz4"), "the snippet must name the Quartz4 assembly");

        using XmlApplicationContext context = new XmlApplicationContext(
            true, "readme", true, null, Array.Empty<string>(), new IResource[] { new StringResource(xml) });

        IScheduler scheduler = (IScheduler) context.GetObject("scheduler");
        SimpleTriggerObject trigger = (SimpleTriggerObject) context.GetObject("exportTrigger");
        Assert.That(scheduler.Status, Is.EqualTo(SchedulerStatus.Running), "the scheduler starts when the context is refreshed");
        Assert.That(trigger.Key, Is.EqualTo(new TriggerKey("exportTrigger", TriggerKey.DefaultGroup)));
        Assert.That(trigger.MisfireInstruction, Is.EqualTo(SimpleTriggerMisfireInstruction.NextWithRemainingCount));
        Assert.That(trigger.RepeatCount, Is.EqualTo(10));
        Assert.That(trigger.RepeatInterval, Is.EqualTo(TimeSpan.FromSeconds(1)));

        // the first call is made at once, the others every second
        QuartzTestObject service = (QuartzTestObject) context.GetObject("exportService");
        SpinWait.SpinUntil(() => service.ExportCount > 0, TimeSpan.FromSeconds(10));
        Assert.That(service.ExportCount, Is.GreaterThan(0));
        Assert.That(trigger.JobKey, Is.Not.Null);
    }
}
