using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleCourier.WebApi.Requests
{
	public class ExecuteReq
	{
		public string Url { get; set; }
		public int Count { get; set; }
	}
}
