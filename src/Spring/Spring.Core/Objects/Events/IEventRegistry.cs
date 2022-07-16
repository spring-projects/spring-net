#region License

/*
 * Copyright � 2002-2011 the original author or authors.
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

namespace Spring.Objects.Events
{
	/// <summary>
	/// A registry that manages subscriptions to and the
	/// publishing of events.
	/// </summary>
	/// <author>Griffin Caprio</author>
	public interface IEventRegistry
	{
		/// <summary>
		/// Publishes <b>all</b> events of the source object.
		/// </summary>
		/// <param name="sourceObject">
		/// The source object containing events to publish.
		/// </param>
		void PublishEvents(object sourceObject);

		/// <summary>
		/// Subscribes to <b>all</b> events published, if the subscriber
		/// implements compatible handler methods.
		/// </summary>
		/// <param name="subscriber">The subscriber to use.</param>
		void Subscribe(object subscriber);

		/// <summary>
		/// Subscribes to the published events of all objects of a given
		/// <see cref="System.Type"/>, if the subscriber implements
		/// compatible handler methods.
		/// </summary>
		/// <param name="subscriber">The subscriber to use.</param>
		/// <param name="targetSourceType">
		/// The target <see cref="System.Type"/> to subscribe to.
		/// </param>
		void Subscribe(object subscriber, Type targetSourceType);

        /// <summary>
        /// Unsubscribes to <b>all</b> events published, if the subscriber
        /// implmenets compatible handler methods.
        /// </summary>
        /// <param name="subscriber">The subscriber to use</param>
	    void Unsubscribe(object subscriber);

        /// <summary>
        /// Unsubscribes to the published events of all objects of a given
        /// <see cref="System.Type"/>, if the subscriber implements
        /// compatible handler methods.
        /// </summary>
        /// <param name="subscriber">The subscriber to use.</param>
        /// <param name="targetSourceType">
        /// The target <see cref="System.Type"/> to unsubscribe from
        /// </param>
        void Unsubscribe(object subscriber, Type targetSourceType);
	}
}
