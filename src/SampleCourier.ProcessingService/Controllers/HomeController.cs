using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SampleCourier.ProcessingService.Controllers
{
	[Produces("application/json")]
	[Route("")]
	public class HomeController : Controller
	{
		// GET: api/Home
		[HttpGet("")]
		[Route("api")]
		[Route("api/home")]
		public IActionResult Get() => Content("Hello from SampleCourier.ProcessingService!");
	}
}
