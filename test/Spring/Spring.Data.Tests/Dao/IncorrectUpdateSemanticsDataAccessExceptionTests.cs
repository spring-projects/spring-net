using System.Runtime.Serialization;
using NUnit.Framework;

namespace Spring.Dao;

[TestFixture]
public class IncorrectUpdateSemanticsDataAccessExceptionTests
{
    [Serializable]
    public class SampleException : IncorrectUpdateSemanticsDataAccessException
    {
        public SampleException() : base() { }
        public SampleException(string message) : base(message) { }
        public SampleException(string message, Exception inner) : base(message, inner) { }
#pragma warning disable SYSLIB0051 // the base serialization constructor is obsolete on net10.0
        protected SampleException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#pragma warning restore SYSLIB0051

        public override bool DataWasUpdated
        {
            get
            {
                return true;
            }
        }
    }
}
