using System;

namespace SampleCourier.Activities.Retrieve
{
	public interface RetrieveArguments
	{
		/// <summary>
		/// The requestId for eventing
		/// </summary>
		Guid RequestId { get; }

		/// <summary>
		/// The address of the content to retrieve
		/// </summary>
		Uri Address { get; }
	}
}