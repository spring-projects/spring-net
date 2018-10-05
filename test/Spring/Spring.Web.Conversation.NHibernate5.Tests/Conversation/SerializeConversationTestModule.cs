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

using System;
using System.Collections.Generic;
using System.Web;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using System.Reflection;

namespace Spring.Web.Conversation
{
    /// <summary>
    /// Module that forces the serialization and deserialization of the session content to simulate a clustered server for 
    /// <see cref="WebConversationStateTest.SerializeConversationTest"/>.
    /// </summary>
    public class SerializeConversationTestModule: IHttpModule
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(SerializeConversationTestModule));

        #endregion

        /// <summary>
        /// Serialized Session Content.
        /// </summary>
        private static MemoryStream SerializedSessionContentStream = new MemoryStream();

        #region IHttpModule Members

        /// <summary>
        /// TODO:
        /// </summary>
        public void Dispose()
        {
            //
        }

        /// <summary>
        /// TODO:
        /// </summary>
        /// <param name="context"></param>
        public void Init(HttpApplication context)
        {
            context.PreRequestHandlerExecute += context_PreRequestHandlerExecute;
            context.PostRequestHandlerExecute += context_PostRequestHandlerExecute;
        }

        /// <summary>
        /// Repopulates the session from the previously serialized content.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void context_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session != null)
            {
                if (HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath == "~/SerializeConversationTest.aspx")
                {
                    if (SerializedSessionContentStream.Length > 0)
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Binder = new MyBinder();
                        //SurrogateSelector surrogateSelector = new SurrogateSelector();
                        //surrogateSelector.AddSurrogate(
                        //    typeof(Object),
                        //    new StreamingContext(StreamingContextStates.All),
                        //    new MySerializationSurrogate());
                        //bf.SurrogateSelector = surrogateSelector;

                        SerializedSessionContentStream.Seek(0, SeekOrigin.Begin);
                        Dictionary<string, object> sessionConttent =
                            (Dictionary<string, object>)bf.Deserialize(SerializedSessionContentStream);

                        HttpContext.Current.Session.Clear();
                        foreach (String keyItem in sessionConttent.Keys)
                        {
                            HttpContext.Current.Session[keyItem] = sessionConttent[keyItem];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Serializes and clears the session.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void context_PostRequestHandlerExecute(object sender, EventArgs e)
        {
            if (HttpContext.Current.Session != null)
            {
                //NHibernate.ISession ss;
                if (HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath == "~/SerializeConversationTest.aspx")
                {
                    Dictionary<string, object> sessionConttent = new Dictionary<string, object>();
                    foreach (String keyItem in HttpContext.Current.Session.Keys)
                    {
                        sessionConttent[keyItem] = HttpContext.Current.Session[keyItem];
                    }

                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Binder = new MyBinder();
                    //SurrogateSelector surrogateSelector = new SurrogateSelector();
                    //surrogateSelector.AddSurrogate(
                    //    typeof(Object), 
                    //    new StreamingContext(StreamingContextStates.All),
                    //    new MySerializationSurrogate());
                    //bf.SurrogateSelector = surrogateSelector;

                    SerializedSessionContentStream = new MemoryStream();
                    bf.Serialize(SerializedSessionContentStream, sessionConttent);

                    HttpContext.Current.Session.Clear();
                }
            }
        }
        #endregion
    }

    public class MyBinder : SerializationBinder
    {
        #region Logging

        private Common.Logging.ILog LOG 
        {
            get
            {
                return Common.Logging.LogManager.GetLogger(typeof(SerializeConversationTestModule));
            }
        }

        #endregion

        public override Type BindToType(string assemblyName, string typeName)
        {
            if (LOG.IsDebugEnabled)
                LOG.Debug(String.Format("MyBinder.BindToType: {0}, {1}", typeName, assemblyName));
            return Type.GetType(typeName + ", " + assemblyName);
        }
    }

    ///// <summary>
    ///// For debugging purpose.
    ///// </summary>
    //public class MySurrogateSelectorWrapper : ISurrogateSelector
    //{

    //    ISurrogateSelector wrapped;
    //    public MySurrogateSelectorWrapper(ISurrogateSelector wrapped)
    //    {
    //        this.wrapped = wrapped;
    //    }
    //    #region ISurrogateSelector Members

    //    public void ChainSelector(ISurrogateSelector selector)
    //    {
    //        this.wrapped.ChainSelector(selector);
    //    }

    //    public ISurrogateSelector GetNextSelector()
    //    {
    //        ISurrogateSelector selector = this.wrapped.GetNextSelector();
    //        if (!(selector is MySurrogateSelectorWrapper))
    //            selector = new MySurrogateSelectorWrapper(selector);
    //        return selector;
    //    }

    //    public ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
    //    {
    //        ISerializationSurrogate surrogate = new MySerializationSurrogate(this.wrapped.GetSurrogate(type, context, out selector));
    //        if (!(selector is MySurrogateSelectorWrapper))
    //            selector = new MySurrogateSelectorWrapper(selector);
    //        return surrogate;
    //    }

    //    #endregion
    //}

    /// <summary>
    /// For debugging purpose.
    /// </summary>
    public class MySerializationSurrogate : ISerializationSurrogate
    {
        #region Logging

        private static readonly Common.Logging.ILog LOG = Common.Logging.LogManager.GetLogger(typeof(SerializeConversationTestModule));

        #endregion

        #region ISerializationSurrogate Members

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            if (LOG.IsDebugEnabled)
                LOG.Debug(String.Format("MySerializationSurrogateWrapper.GetObjectData({0},...", obj.GetType()));

            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (obj is ISerializable)
            {
                ((ISerializable)obj).GetObjectData(info, context);
            }
            else
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    if ((fields[i].Attributes & FieldAttributes.NotSerialized) == 0)
                    {
                        info.AddValue(fields[i].Name, fields[i].GetValue(obj), fields[i].FieldType);
                    }
                }
            }
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            if (LOG.IsDebugEnabled)
                LOG.Debug(String.Format("MySerializationSurrogateWrapper.SetObjectData({0},...", obj.GetType()));

            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            for (int i = 0; i < fields.Length; i++)
            {
                if ((fields[i].Attributes & FieldAttributes.NotSerialized) == 0)
                {
                    fields[i].SetValue(obj, info.GetValue(fields[i].Name, fields[i].FieldType));
                }
            }
            if (obj is IDeserializationCallback)
                ((IDeserializationCallback)obj).OnDeserialization(obj);

            return obj;
        }

        #endregion
    }
}
