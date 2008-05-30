using System;
using System.Runtime.Serialization;
using NUnit.Framework;
namespace Spring.Dao
{
	[TestFixture]
	public class UncategorizedDataAccessExceptionTests
	{
		[Serializable]
			public class SampleException : UncategorizedDataAccessException
		{
			public SampleException() : base() {}
			public SampleException( string message ) : base( message ) {}
			public SampleException( string message, Exception inner ) : base( message, inner ) {}
			protected SampleException( SerializationInfo info, StreamingContext context ) : base( info, context ) {}
		}
	}
}
