﻿using System;

namespace SampleCourier.Activities.Validate
{
	public interface ValidateArguments
	{
		/// <summary>
		/// The requestId for this activity argument
		/// </summary>
		Guid RequestId { get; }

		/// <summary>
		/// The address of the resource to retrieve
		/// </summary>
		Uri Address { get; }
	}
}