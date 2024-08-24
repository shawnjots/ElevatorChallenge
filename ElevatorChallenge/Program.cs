using Autofac;
using ElevatorChallenge;


var container = ContainerConfig.Configure();
using(var scope = container.BeginLifetimeScope())
{
	var app = scope.Resolve<ElevatorSimulation>();
	app.Run();
}
