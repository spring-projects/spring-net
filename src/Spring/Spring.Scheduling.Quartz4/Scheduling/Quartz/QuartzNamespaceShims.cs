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

// Quartz 4 moved IJobFactory, IThreadPool, TriggerFiredBundle and the rest of Quartz.Spi into Quartz.Extensibility.
// Sources that are compiled from Spring.Scheduling.Quartz3 still say "using Quartz.Spi;", and an empty declaration of the
// old namespace lets them compile unchanged instead of carrying an #if around each using directive.
namespace Quartz.Spi
{
}
