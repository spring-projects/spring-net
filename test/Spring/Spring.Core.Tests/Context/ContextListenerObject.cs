using Spring.Context.Events;

#region License

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

#endregion

namespace Spring.Context
{
	/// <summary>
	/// Test object for receiving application context events.
	/// </summary>
	/// <author>Mark Pollack</author>
	public sealed class ContextListenerObject : IApplicationContextAware, IApplicationEventListener
	{
		private IApplicationContext _ctx;
		private bool _appListenerContextRefreshed = false;
		private bool _appListenerContextClosed = false;
		private bool _ctxRefreshed = false;
		private bool _ctxClosed = false;

		public ContextListenerObject()
		{
		}

		public IApplicationContext ApplicationContext
		{
			set
			{
				_ctx = value;
				_ctx.ContextEvent += new ApplicationEventHandler(ContextRefreshedHandler);
			}
			get { return _ctx; }
		}

		public bool AppListenerContextRefreshed
		{
			get { return _appListenerContextRefreshed; }
		}

		public bool AppListenerContextClosed
		{
			get { return _appListenerContextClosed; }
		}

		public bool CtxRefreshed
		{
			get { return _ctxRefreshed; }
		}

		public bool CtxClosed
		{
			get { return _ctxClosed; }
		}

		public void HandleApplicationEvent(object source, ApplicationEventArgs e)
		{
			ContextEventArgs ctxArgs = e as ContextEventArgs;
			if (ctxArgs != null)
			{
				if (ctxArgs.Event == ContextEventArgs.ContextEvent.Refreshed)
				{
					_appListenerContextRefreshed = true;
				}
				if (ctxArgs.Event == ContextEventArgs.ContextEvent.Closed)
				{
					_appListenerContextClosed = true;
				}
			}
		}

		private void ContextRefreshedHandler(object sender, ApplicationEventArgs e)
		{
			ContextEventArgs args = e as ContextEventArgs;
			if (args != null)
			{
				if (args.Event == ContextEventArgs.ContextEvent.Refreshed)
				{
					_ctxRefreshed = true;
				}
				else if (args.Event == ContextEventArgs.ContextEvent.Closed)
				{
					_ctxClosed = true;
				}
			}
		}
	}
}