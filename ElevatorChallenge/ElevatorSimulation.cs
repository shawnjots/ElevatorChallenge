using Autofac;
using ElevatorChallenge.DTOs.Passenger;
using ElevatorChallenge.Events;
using ElevatorChallenge.Util;
using ElevatorChallenge.Util.Interfaces;
using ElevatorSimulator.Utilities;
using EventChallenge.Services;
using EventChallenge.Services.Interfaces;
using static ElevatorChallenge.Events.ElevatorEventArgs;
using IMapper = EventChallenge.Services.Interfaces.IMapper;
using Mapper = EventChallenge.Services.Mappers.Mapper;

namespace ElevatorChallenge
{
	internal class ElevatorSimulation
	{
		IContainer container = ContainerConfig.Configure();
		IElevatorService _elevatorService;
		ILogger _logger;
		Dictionary<int, int> startingFloorWeights = new();
		Dictionary<int, int> destinationFloorWeights = new();
		DateTime simulationStartTime = new DateTime();
		double timeScaleFactor = 60.0;
		Random random = new Random();
		

		public ElevatorSimulation(IElevatorService elevatorService, ILogger logger)
		{
			_elevatorService = elevatorService;
			_elevatorService.ElevatorEvents.Subscribe(UpdateElevatorDisplay);
			_elevatorService.PassengerEvents.Subscribe(UpdatePassengerDisplay);
			_logger = logger;
		
		}

		public async void Run()
		{
			// Run the console interface
			Console.WriteLine("Elevator Simulator Console Interface");
			Console.WriteLine("------------------------------------");


			while (true)
			{
				Console.WriteLine("\nOptions:");
				Console.WriteLine("1. Add Passenger to Waiting Queue & Elevator Dispatch");
				Console.WriteLine("2. Run Simulation");
				Console.WriteLine("3. Exit");

				Console.Write("Enter your choice (1-3): ");
				string choice = Console.ReadLine();
				if (choice == null)
				{
					Console.WriteLine("Invalid input. Please enter a valid option.");
					choice = "0";
				}

				switch (choice)
				{
					case "1":
						AddPassengerToQueueElevatorDispatch(_elevatorService);
						break;

					case "2":
						await RunSimulationAsync(_elevatorService);
						break;

					case "3":
						Console.WriteLine("Exiting the program. Goodbye!");
						return;

					case null:
						Console.WriteLine("Invalid input. Please enter a valid option.");
						break;

					default:
						Console.WriteLine("Invalid choice. Please enter a valid option.");
						break;
				}
			}






}


IElevatorService InitialiseElevatorService()
{
	var builder = new ContainerBuilder();
	builder.RegisterType<ElevatorService>().As<IElevatorService>();
	builder.RegisterType<Mapper>().As<IMapper>();

	var container = builder.Build();


	var mapper = container.Resolve<IMapper>();

	return container.Resolve<IElevatorService>();

}

ILogger InitialiseLogger()
{
	var builder = new ContainerBuilder();

	builder.RegisterType<Logger>().As<ILogger>();

	var container = builder.Build();


	return container.Resolve<ILogger>();

}

void UpdateElevatorDisplay(ElevatorEventArgs args)
{
	switch (args.NewStatus)
	{
		case Status.Moving:
			_logger.LogInformation($"Elevator ID: {args.ElevatorId}, Moved to floor {args.CurrentFloor}");
			break;
		default:
			_logger.LogInformation($"Elevator {args.ElevatorId}: {args.NewStatus} (Floor {args.CurrentFloor})");
			break;
	}

}

void UpdatePassengerDisplay(PassengerEventArgs args)
{
	switch (args.PassengerStatus)
	{
		case PassengerEventArgs.Status.DepartedElevator:
			_logger.LogInformation($" {args.PassengerCount} passengers departed onto Elevator {args.ElevatorId} at floor {args.CurrentFloor}");
			break;
		case PassengerEventArgs.Status.BoardedElevator:
			_logger.LogInformation($" {args.PassengerCount} passengers boarded onto Elevator {args.ElevatorId} at floor {args.CurrentFloor}");
			break;
	}
}

void AddPassengerToQueueElevatorDispatch(IElevatorService elevatorService)
{
	Console.Write("Enter the floor for the waiting passenger: ");
	if (int.TryParse(Console.ReadLine(), out int floor))
	{
		Console.Write("Enter the passenger's destination floor: ");
		if (int.TryParse(Console.ReadLine(), out int destinationFloor))
		{
			if (destinationFloor == floor)
			{
				Console.WriteLine("The destination floor cannot be the same as the starting floor.");
			}
			else if (destinationFloor < Constant.MinFloor || destinationFloor > Constant.MaxFloor)
			{
				Console.WriteLine($"Invalid destination floor. Please enter a floor within the building's range ({Constant.MinFloor} - {Constant.MaxFloor}).");
			}
			else
			{
				var passengerDto = new PassengerDTO { DestinationFloor = destinationFloor };
				elevatorService.QueuePassenger(floor, passengerDto);
			}
		}
		else
		{
			Console.WriteLine("Invalid destination floor. Please enter a valid number.");
		}
	}
	else
	{
		Console.WriteLine("Invalid floor. Please enter a valid number.");
	}
}

async Task RunSimulationAsync(IElevatorService elevatorService)
{
	Console.WriteLine("Running Simulation...");

	// Set up a timer to trigger the simulation every second
	var timer = new Timer(
		async _ =>
		{
			SimulateRandomElevatorActivity(elevatorService);
			await Task.Yield(); // Ensure asynchronous operation
		},
		null,
		TimeSpan.Zero,
		TimeSpan.FromSeconds(30));

	// Wait indefinitely
	await Task.Delay(Timeout.Infinite);
}

void SimulateRandomElevatorActivity(IElevatorService elevatorService)
{
	Random random = new();

	var timer = new System.Timers.Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);
	timer.AutoReset = true;
	timer.Elapsed += (sender, e) => AdjustSimulationParameters(random);
	AdjustSimulationParameters(random);
	timer.Start();


	while (true)
	{
		// Wait a random amount of time between passenger calls (for variability)
		int timeUntilNextRequest = random.Next((int)TimeSpan.FromSeconds(30).TotalMilliseconds, (int)TimeSpan.FromSeconds(60).TotalMilliseconds);
		Thread.Sleep(timeUntilNextRequest);

		// 1. Determine "activity level" - low, normal, high based on factors
		ActivityLevel currentActivity = DetermineActivityLevel(random);

		// 2. Generate Passengers with Biases 
		for (int i = 0; i < GetPassengerCount(currentActivity); i++)
		{
			int floor = GetStartingFloor(random);
			int destinationFloor = GetDestinationFloor(random, floor);
			var passengerDto = new PassengerDTO { DestinationFloor = destinationFloor };
			elevatorService.QueuePassenger(floor, passengerDto);
		}
	}
}

ActivityLevel DetermineActivityLevel(Random random)
{
	DateTime simulatedTime = GetSimulatedTime(); // Retrieve current simulated time 

	// 1. Base Traffic Levels based on Time
	if (IsNightTime(simulatedTime))
	{
		return ActivityLevel.Low;
	}
	else if (IsRushHour(simulatedTime))
	{
		return ActivityLevel.High;
	}
	else // Normal hours
	{
		// 2. Variability (within Normal Hours)
		int baseActivityValue = random.Next(30, 70); // Example: Range from 30-70% within normal hours

		// 3. Special Events or Dynamic Adjustment (Optional)
		if (IsSpecialEvent())
		{
			baseActivityValue += 20; // Boost if simulating a special event
		}

		// 4. Map Value to Activity Level 
		if (baseActivityValue <= 40)
		{
			return ActivityLevel.Low;
		}
		else if (baseActivityValue <= 80)
		{
			return ActivityLevel.Medium;
		}
		else
		{
			return ActivityLevel.High;
		}
	}
}

bool IsNightTime(DateTime simulatedTime)
{
	int nightStartHour = 21; // 9 PM
	int nightEndHour = 5; // 5 AM

	// We'll consider nighttime as being either late in the evening *or* very early
	return (simulatedTime.Hour >= nightStartHour || simulatedTime.Hour < nightEndHour);
}

bool IsRushHour(DateTime simulatedTime)
{
	// Morning Rush Configuration
	int morningRushStartHour = 7;
	int morningRushEndHour = 9;

	// Evening Rush Configuration
	int eveningRushStartHour = 16; // 4 PM
	int eveningRushEndHour = 18; // 6 PM

	return (simulatedTime.Hour >= morningRushStartHour && simulatedTime.Hour < morningRushEndHour) ||
		   (simulatedTime.Hour >= eveningRushStartHour && simulatedTime.Hour < eveningRushEndHour);
}

bool IsSpecialEvent()
{
	return random.NextDouble() < 0.02; // 2% chance of an event
}

DateTime GetSimulatedTime()
{
	if (simulationStartTime == DateTime.MinValue) // Initialize on first call
	{
		simulationStartTime = DateTime.Now;
	}

	TimeSpan elapsedRealTime = DateTime.Now - simulationStartTime;
	TimeSpan simulatedTime = TimeSpan.FromSeconds(elapsedRealTime.TotalSeconds * timeScaleFactor);

	return simulationStartTime + simulatedTime; // Adjust by the scaled-up timeframe
}

int GetPassengerCount(ActivityLevel activityLevel)
{
	Random random = new Random(); // Instance for randomness within ranges

	switch (activityLevel)
	{
		case ActivityLevel.Low:
			return random.Next(1, 3); // Smaller groups or individuals

		case ActivityLevel.Medium:
			return random.Next(2, 5); // Typical small group sizes

		case ActivityLevel.High:
			return random.Next(3, 7); // Larger potential groups when it's busy

		default:
			return 1; // Safe default in case of unexpected input 
	}
}

int GetStartingFloor(Random random)
{
	// Ensure you have initialized startingFloorWeights elsewhere (likely in AdjustSimulationParameters)
	if (startingFloorWeights.Count == 0)
	{
		throw new InvalidOperationException("Floor weights have not been configured.");
	}

	int totalWeight = startingFloorWeights.Values.Sum(); // Get the sum of all the weights

	int randomValue = random.Next(1, totalWeight + 1); // 1 to inclusive of totalWeight
	int currentWeight = 0;

	// Find the floor associated with the random value:
	foreach (var floorAndWeight in startingFloorWeights)
	{
		currentWeight += floorAndWeight.Value;
		if (randomValue <= currentWeight)
		{
			return floorAndWeight.Key; // Return the floor number
		}
	}

	// This should be rarely hit - If due to rounding issues, return a random valid floor
	return startingFloorWeights.Keys.ElementAt(random.Next(startingFloorWeights.Count));
}


int GetDestinationFloor(Random random, int startingFloor)
{
	// Similar to starting floors, ensure destinationFloorWeights is initialized elsewhere
	if (destinationFloorWeights.Count == 0)
	{
		throw new InvalidOperationException("Destination floor weights not configured.");
	}

	// We'll repeat the same weighted random selection as used for GetStartingFloor

	int candidateFloor = 0;
	do
	{
		int totalWeight = destinationFloorWeights.Values.Sum();
		int randomValue = random.Next(1, totalWeight + 1);
		int currentWeight = 0;

		foreach (var floorAndWeight in destinationFloorWeights)
		{
			currentWeight += floorAndWeight.Value;
			if (randomValue <= currentWeight)
			{
				candidateFloor = floorAndWeight.Key;
				break; // We have a potential floor
			}
		}
	} while (candidateFloor == startingFloor); // Ensure it's not the starting floor itself

	return candidateFloor;
}

void AdjustSimulationParameters(Random random)
{
	DateTime simulatedTime = GetSimulatedTime();
	int rushHourIntensity = 70; // Range 0-100, Higher means stronger rush hour effect

	int midBuildingFloor = (Constant.MaxFloor - Constant.MinFloor) / 2;

	// Change factors based on simulated time 
	if (IsRushHour(simulatedTime))
	{
		// Starting Floors (Strong downward bias)
		startingFloorWeights.Clear();
		for (int i = Constant.MinFloor; i <= Constant.MaxFloor; i++)
		{
			// Decrease drastically the higher the floor
			startingFloorWeights[i] = rushHourIntensity - (i * 2);
		}

		// Destinations (Ground floor heavily favored)
		destinationFloorWeights.Clear();
		destinationFloorWeights[Constant.MinFloor] = rushHourIntensity;

		// Mid floors get a slight favor
		int midFloor = (Constant.MaxFloor + Constant.MinFloor) / 2;
		destinationFloorWeights[midFloor] = rushHourIntensity / 3;
	}
	else
	{
		startingFloorWeights.Clear(); // Or reset an existing dictionary
		startingFloorWeights[Constant.MinFloor] = 40; // Higher chance for lobby

		// Give mid-range floors a mild bonus
		startingFloorWeights[midBuildingFloor - 1] = 15;
		startingFloorWeights[midBuildingFloor] = 15;
		startingFloorWeights[midBuildingFloor + 1] = 15;

		// Destinations (Ground floor heavily favored)
		destinationFloorWeights.Clear();
		destinationFloorWeights[Constant.MinFloor] = rushHourIntensity;

		// Mid floors get a slight favor
		int midFloor = (Constant.MaxFloor + Constant.MinFloor) / 2;
		destinationFloorWeights[midFloor] = rushHourIntensity / 3;

		// Very slightly nudge remaining weights upward compared to totally uniform
		for (int i = Constant.MinFloor + 1; i < Constant.MaxFloor; i++)
		{
			if (!startingFloorWeights.ContainsKey(i))
			{
				startingFloorWeights[i] = 5;
			}
		}


	}
}

	}
}