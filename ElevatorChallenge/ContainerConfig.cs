using Autofac;
using EventChallenge.Services.Interfaces;
using EventChallenge.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventChallenge.Services.Mappers;
using ElevatorSimulator.Utilities;
using ElevatorChallenge.Util.Interfaces;

namespace ElevatorChallenge
{
	public static class ContainerConfig
	{
		public static IContainer Configure()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<ElevatorSimulation>();
			builder.RegisterType<ElevatorService>().As<IElevatorService>();
			builder.RegisterType<Mapper>().As<IMapper>();
			builder.RegisterType<Logger>().As<ILogger>();
			return builder.Build();
		}
	}
}