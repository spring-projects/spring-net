using System.Runtime.Serialization;
using NUnit.Framework;
namespace Spring.Dao
{
	[TestFixture]
	public class IncorrectUpdateSemanticsDataAccessExceptionTests
	{
		[Serializable]
		public class SampleException : IncorrectUpdateSemanticsDataAccessException
		{
			public SampleException() : base() {}
			public SampleException( string message ) : base( message ) {}
			public SampleException( string message, Exception inner ) : base( message, inner ) {}
			protected SampleException( SerializationInfo info, StreamingContext context ) : base( info, context ) {}
			public override bool DataWasUpdated
			{
				get
				{
					return true;
				}
			}
		}
	}
}
