using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dmo.MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SampleCourier.WebApi.Config;

namespace SampleCourier.WebApi
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{			
			services.AddMassTransitWithRabbitMq(Configuration);

			services.AddSwaggerGen(opts =>
			{
				opts.DescribeAllEnumsAsStrings();

				opts.SwaggerDoc("v1",new Swashbuckle.AspNetCore.Swagger.Info
				{
					Title = "SampleCourier.WebApi HTTP API",
					Version = "v1",
					Description = "SampleCourier.WebApi HTTP API",
					TermsOfService = "Terms Of Service"
				});

				// Set the comments path for the Swagger JSON and UI.
				var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory,xmlFile);
				opts.IncludeXmlComments(xmlPath);
			});

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app,IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseExceptionHandler("/api/error");

			app.UseSwagger()
				.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint($"/swagger/v1/swagger.json","V1 Docs");
				});
			
			app.UseMassTransit();
			app.UseMvc();
		}
	}
}
