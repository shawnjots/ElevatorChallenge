using AutoMapper;
using ElevatorChallenge.DTOs.Passenger;
using ElevatorChallenge.Events;
using ElevatorChallenge.Models;
using ElevatorChallenge.Util;
using EventChallenge.Services.Interfaces;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using static ElevatorChallenge.Models.Elevator;

namespace EventChallenge.Services
{
	public class ElevatorService : IElevatorService
	{
		private readonly IMapper _mapper;
		private readonly object _lock = new();
		private readonly ConcurrentBag<Elevator> _elevators;
		private readonly ConcurrentDictionary<int, Floor> _floors;
		private readonly ConcurrentBag<Passenger> _passengers = new ConcurrentBag<Passenger>();
		private readonly ConcurrentQueue<int> _requestCount;
		private readonly Subject<PassengerEventArgs> _passengerStatusSubject = new();
		public IObservable<PassengerEventArgs> PassengerEvents => _passengerStatusSubject.AsObservable();
		private readonly Subject<ElevatorEventArgs> _elevatorStatusSubject = new();
		public IObservable<ElevatorEventArgs> ElevatorEvents => _elevatorStatusSubject.AsObservable();

		/// <summary>
        /// Initializes a new instance of the ElevatorService class.
        /// </summary>
        /// <param name="mapper">The mapper instance to map objects.</param>
        /// <returns>void</returns>	
		public ElevatorService(IMapper mapper)
		{
			_requestCount = new ConcurrentQueue<int>();
			_elevators = new ConcurrentBag<Elevator>();
			_floors = new ConcurrentDictionary<int, Floor>();

			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

			EnableElevators();
			EnableFloors();

			PassengerEvents.Subscribe(async passengerArgs =>
			{
				if (passengerArgs.PassengerStatus == PassengerEventArgs.Status.AddedToQueue)
				{
					_requestCount.Enqueue(passengerArgs.CurrentFloor); 
					await ManageElevators();
				}
			});
		}

		/// <summary>
		/// Initializes a new instance of the ElevatorService class.
		/// </summary>
		/// <param name="mapper">The mapper instance to map objects.</param>
		/// <param name="elevators">The list of elevators to be used.</param>
		/// <param name="floors">The list of floors to be used.</param>
		/// <returns>void</returns>
		public void QueuePassenger(int floor, PassengerDTO passengerDTO)
		{
			lock (_lock)
            {
                var passenger = _mapper.Map<PassengerDTO, Passenger>(passengerDTO);

                if (!_floors.ContainsKey(floor))
                {
                    _floors[floor] = new Floor(floor); // Fixed: Initialize the floor if not exist
                }
                _floors[floor].ScheduledPassengers.Enqueue(passenger);

                _passengerStatusSubject.OnNext(new PassengerEventArgs()
                {
                    CurrentFloor = floor,
                    ElevatorId = -1,
                    PassengerCount = -1,
                    PassengerStatus = PassengerEventArgs.Status.AddedToQueue
                });
            }
		}

		/// <summary>
        /// Initializes all floors within the range defined by constants.
        /// </summary>
        /// <returns>void</returns>
		private void EnableFloors()
		{
			for (int i = Constant.MinFloor; i <= Constant.MaxFloor; i++)
			{
				_floors[i] = new Floor(i);
			}
		}

		/// <summary>
        /// Initializes all elevators within the range defined by constants.
        /// </summary>
        /// <returns>void</returns>
		private void EnableElevators()
		{
			for (int i = 1; i <= Constant.MaxElevators; i++)
			{
				_elevators.Add(new Elevator(Constant.MaxPassengers));
			}
		}

		/// <summary>
        /// Manages the elevators to handle passenger requests.
        /// </summary>
        /// <returns>Task</returns>
		private async Task ManageElevators()
		{
			foreach (var elevator in _elevators.Where(e => e.CurrentStatus == Elevator.Status.Stationary))
			{

				await ExecuteNextStep(elevator);
			}

		}

		/// <summary>
        /// Executes the next step for a given elevator.
        /// </summary>
        /// <param name="elevator">The elevator to manage.</param>
        /// <returns>Task</returns>
		private async Task ExecuteNextStep(Elevator elevator)
		{
			
			List<int> floorsWithWaiting = GetFloorsWithWaitingPassengers(elevator);

			
			int bestFloor = FindNextBestFloor(elevator, floorsWithWaiting);

			
			await SendElevator(elevator, bestFloor);
		}

		/// <summary>
        /// Gets the list of floors with waiting passengers for a given elevator.
        /// </summary>
        /// <param name="elevator">The elevator to check.</param>
        /// <returns>List of floor numbers</returns>
		private List<int> GetFloorsWithWaitingPassengers(Elevator elevator)
		{
			var query = _floors.Where(kvp => kvp.Value.ScheduledPassengers.Any());

			if (elevator.CurrentDirection == Direction.Up)
			{
				query = query.Where(kvp => kvp.Key > elevator.CurrentFloor);
			}
			else if (elevator.CurrentDirection == Direction.Down)
			{
				query = query.Where(kvp => kvp.Key < elevator.CurrentFloor);
			}

			
			return query.Select(kvp => kvp.Key).ToList();
		}

		/// <summary>
        /// Finds the next best floor for a given elevator to move to.
        /// </summary>
        /// <param name="elevator">The elevator to manage.</param>
        /// <param name="floorsWithWaiting">List of floors with waiting passengers.</param>
        /// <returns>Best floor number</returns>
		private int FindNextBestFloor(Elevator elevator, List<int> floorsWithWaiting)
		{
			if (floorsWithWaiting.Count == 0)
			{
				return elevator.CurrentFloor;
			}
			int bestFloor = floorsWithWaiting.First();
			int highestScore = 0;
			foreach (var floor in floorsWithWaiting)
			{
				int score = CalculateAttractivenessScore(elevator, floor);
				if (score > highestScore)
				{
					highestScore = score;
					bestFloor = floor;
				}
			}

			return bestFloor;
		}

		/// <summary>
        /// Calculates the attractiveness score for a given floor.
        /// </summary>
        /// <param name="elevator">The elevator to manage.</param>
        /// <param name="floor">The floor number to evaluate.</param>
        /// <returns>Attractiveness score</returns>
		private int CalculateAttractivenessScore(Elevator elevator, int floor)
		{
			int baseScore = 20;
			int densityBonus = CalculateDensityBonus(floor, elevator);
			int destinationBonus = CalculateDestinationBonus(elevator, floor);
			double distanceFactor = GetDistanceFactor(elevator, floor);
			double competitionFactor = GetCompetitionFactor(floor);
			int waitSeverityScore = CalculateWaitSeverityScore(elevator, floor);
			double waitFactor = waitSeverityScore * Constant.WaitFactor;
			int adjustedScore = (int)(baseScore + densityBonus + destinationBonus +
									  distanceFactor + competitionFactor + waitFactor);
			return adjustedScore;
		}

		/// <summary>
        /// Calculates the density bonus for a given floor.
        /// </summary>
        /// <param name="floor">The floor number to evaluate.</param>
        /// <param name="elevator">The elevator to manage.</param>
        /// <returns>Density bonus score</returns>
		private int CalculateDensityBonus(int floor, Elevator elevator)
		{
			int numberOfWaitingPassengers = _floors[floor].ScheduledPassengers.Count;
			int availableSpaces = elevator.MaximumCapacity - elevator.Passengers.Count;
			int adjustedBonus = Math.Min(numberOfWaitingPassengers, availableSpaces) * 5;
			return adjustedBonus;
		}

		/// <summary>
        /// Calculates the destination bonus for a given floor.
        /// </summary>
        /// <param name="elevator">The elevator to manage.</param>
        /// <param name="floor">The floor number to evaluate.</param>
        /// <returns>Destination bonus score</returns>
		private int CalculateDestinationBonus(Elevator elevator, int floor)
		{
			if (elevator.Passengers.Any(p => p.DestinationFloor == floor))
			{
				return 15;
			}
			int totalDistanceDifference = elevator.Passengers
											  .Select(p => Math.Abs(p.DestinationFloor - floor))
											  .Sum();
			int averageDifference = 0;
			try
			{
				averageDifference = totalDistanceDifference / elevator.Passengers.Count;
			}
			catch (DivideByZeroException)
            {
				averageDifference = 0;
			}
			int proximityBonus = 20 - averageDifference;
			return proximityBonus;
		}

		/// <summary>
        /// Calculates the wait severity score for a given floor.
        /// </summary>
        /// <param name="elevator">The elevator to manage.</param>
        /// <param name="floor">The floor number to evaluate.</param>
        /// <returns>Wait severity score</returns>
		private int CalculateWaitSeverityScore(Elevator elevator, int floor)
		{
			double avgWaitTime = _floors[floor].ScheduledPassengers.Average(p => (DateTime.Now - p.RequestedAt).TotalSeconds);
			int numPassengers = _floors[floor].ScheduledPassengers.Count;
			double normalizedWaitTime = avgWaitTime / GetDynamicWaitThreshold(elevator);
			int densityFactor = Math.Min(numPassengers, Constant.DensityLimit);
			int severityScore = (int)(normalizedWaitTime * Constant.WaitFactor + densityFactor * Constant.DensityWeight);
			return severityScore;
		}

		/// <summary>
        /// Gets the dynamic wait threshold for a given elevator.
        /// </summary>
        /// <param name="elevator">The elevator to manage.</param>
        /// <returns>Dynamic wait threshold</returns>
		private double GetDynamicWaitThreshold(Elevator elevator)
		{
			double loadFactor = (double)elevator.Passengers.Count / elevator.MaximumCapacity;
			double adjustedThreshold = Constant.WaitTimeFactor - (loadFactor * Constant.LoadSensitivity);
			return adjustedThreshold;
		}

		/// <summary>
        /// Gets the distance factor for a given floor.
        /// </summary>
        /// <param name="elevator">The elevator to manage.</param>
        /// <param name="floor">The floor number to evaluate.</param>
        /// <returns>Distance factor</returns>
		private double GetDistanceFactor(Elevator elevator, int floor)
		{
			double distance = Math.Abs(elevator.CurrentFloor - floor);
			double maxDistance = Constant.MaxFloor - Constant.MinFloor;
			double normalizedDistance = distance / maxDistance;
			double distanceFactor = -50 * normalizedDistance;
			return distanceFactor;
		}

		/// <summary>
        /// Gets the competition factor for a given floor.
        /// </summary>
        /// <param name="floor">The floor number to evaluate.</param>
        /// <returns>Competition factor</returns>/// <summary>
        /// Gets the competition factor for a given floor.
        /// </summary>
        /// <param name="floor">The floor number to evaluate.</param>
        /// <returns>Competition factor</returns>
		private double GetCompetitionFactor(int floor)
		{
			int nearbyElevators = _elevators.Count(e => Math.Abs(e.CurrentFloor - floor) <= Constant.ProximityFactor);
			double competitionFactor = -10 * nearbyElevators;
			return competitionFactor;
		}

		/// <summary>
        /// Sends an elevator to a specified floor.
        /// </summary>
        /// <param name="elevator">The elevator to manage.</param>
        /// <param name="bestFloor">The floor number to send the elevator to.</param>
        /// <returns>Task</returns>
		private async Task SendElevator(Elevator elevator, int bestFloor)
		{
			elevator.CurrentDirection = _getDirection(elevator, bestFloor);
			elevator.CurrentStatus = Status.Moving;
			await _simulateMovement(elevator, bestFloor);
		}

		/// <summary>
        /// Determines the direction for an elevator to move.
        /// </summary>
        /// <param name="elevator">The elevator to manage.</param>
        /// <param name="bestFloor">The floor number to evaluate.</param>
        /// <returns>Direction</returns>
		private Elevator.Direction _getDirection(Elevator elevator, int bestFloor)
		{
			if (elevator.CurrentFloor < bestFloor)
			{
				return Direction.Up;
			}
			else if (elevator.CurrentFloor > bestFloor)
			{
				return Direction.Down;
			}
			else
			{
				return Direction.None; 
			}
		}

		/// <summary>
        /// Simulates the movement of an elevator to a specified floor.
        /// </summary>
        /// <param name="elevator">The elevator to manage.</param>
        /// <param name="bestFloor">The floor number to move to.</param>
        /// <returns>Task</returns>
		private async Task _simulateMovement(Elevator elevator, int bestFloor)
		{
			int endFloor = 0;
			int startFloor = elevator.CurrentFloor;

			if (elevator.Passengers.Any())
			{
				endFloor = elevator.CurrentDirection == Direction.Up ?
					elevator.Passengers.Max(p => p.DestinationFloor) :
					elevator.Passengers.Min(p => p.DestinationFloor);
			}
			else
			{
				endFloor = bestFloor;
			}

			int floorsPerSecond = 1;
			double timePerFloor = 1.0 / floorsPerSecond;

			for (int currentFloor = startFloor; currentFloor != endFloor; currentFloor += Math.Sign(endFloor - startFloor)) // Simulate moving floor-by-floor
			{
				elevator.CurrentFloor = currentFloor;

				await Task.Delay(TimeSpan.FromSeconds(timePerFloor));
				NotifyElevatorStatusChange(elevator);
			}

			elevator.CurrentFloor = endFloor;
			_handleArrival(elevator);

			await DispatchElevatorToFloorWithLongestWaitingPassenger(elevator, endFloor);
		}

		/// <summary>
        /// Notifies subscribers about the status change of an elevator.
        /// </summary>
        /// <param name="elevator">The elevator to manage.</param>
        /// <returns>void</returns>
		private void NotifyElevatorStatusChange(Elevator elevator)
		{
			_elevatorStatusSubject.OnNext(new ElevatorEventArgs()
			{
				ElevatorId = elevator.Id,
				NewStatus = (ElevatorEventArgs.Status)elevator.CurrentStatus,
				CurrentFloor = elevator.CurrentFloor
			});
		}

		/// <summary>
        /// Handles the arrival of an elevator at a floor.
        /// </summary>
        /// <param name="elevator">The elevator to manage.</param>
        /// <returns>void</returns>
		private void _handleArrival(Elevator elevator)
		{
			
			elevator.CurrentStatus = Status.DoorsOpen; 

			
			var exitingPassengers = elevator.Passengers.Where(p => p.DestinationFloor == elevator.CurrentFloor).ToList();
			foreach (var passenger in exitingPassengers)
			{
				elevator.UnloadPassenger(passenger);
			}

			if (exitingPassengers.Any())
			{
				NotifyPassengerStatusChange(elevator, PassengerEventArgs.Status.DepartedElevator, exitingPassengers.Count);
			}

			
			if (_floors.TryGetValue(elevator.CurrentFloor, out Floor? value)) 
			{
				var queue = value;
				while (queue.ScheduledPassengers.Any() && elevator.Passengers.Count < elevator.MaximumCapacity)
				{
					queue.ScheduledPassengers.TryDequeue(out var passenger);
					if (passenger != null)
						elevator.LoadPassenger(passenger);
				}

				if (elevator.Passengers.Any())
				{
					NotifyPassengerStatusChange(elevator, PassengerEventArgs.Status.BoardedElevator, elevator.Passengers.Count);
				}

			}

			
			if (elevator.Passengers.Any())
			{
				elevator.CurrentDirection = DetermineDirection(elevator);
				elevator.CurrentStatus = Status.Moving;
			}
			else
			{
		
				elevator.CurrentDirection = DetermineDirection(elevator);

				if (_floors.Values.Any(f => f.ScheduledPassengers.Any()))
				{
					elevator.CurrentStatus = Status.Moving; 
				}
				else
				{
					elevator.CurrentStatus = Status.Stationary;
					NotifyElevatorStatusChange(elevator);

				}
			}
		}

		/// <summary>
        /// Dispatches an elevator to the floor with the longest waiting passenger.
        /// </summary>
        /// <param name="elevator">The elevator to manage.</param>
        /// <param name="floor">The floor number to evaluate.</param>
        /// <returns>Task</returns>
		private async Task DispatchElevatorToFloorWithLongestWaitingPassenger(Elevator elevator, int floor)
		{
			if (elevator.Passengers.Any())
			{
				await SendElevator(elevator, floor);
			}
		}

		/// <summary>
        /// Notifies subscribers about the status change of passengers.
        /// </summary>
        /// <param name="elevator">The elevator to manage.</param>
        /// <param name="passengerStatus">The new status of the passengers.</param>
        /// <param name="passengersCount">The number of passengers affected.</param>
        /// <returns>void</returns>
		private void NotifyPassengerStatusChange(Elevator elevator, PassengerEventArgs.Status passengerStatus, int passengersCount)
		{
			_passengerStatusSubject.OnNext(new PassengerEventArgs()
			{
				CurrentFloor = elevator.CurrentFloor,
				ElevatorId = elevator.Id,
				PassengerCount = passengersCount,
				PassengerStatus = passengerStatus
			});
		}

		/// <summary>
        /// Determines the direction for an elevator based on its passengers' destinations.
        /// </summary>
        /// <param name="elevator">The elevator to manage.</param>
        /// <returns>Direction</returns>
		private Elevator.Direction DetermineDirection(Elevator elevator)
		{
			if (elevator.Passengers.Any())
			{
	
				int maxDestination = elevator.Passengers.Max(p => p.DestinationFloor);
				int minDestination = elevator.Passengers.Min(p => p.DestinationFloor);


				if (maxDestination > elevator.CurrentFloor)
				{
					return Direction.Up;
				}
				else if (minDestination < elevator.CurrentFloor)
				{
					return Direction.Down;
				}
				else
				{
					return Direction.None;
				}
			}
			return Direction.None;
		}

		/// <summary>
		/// Adds a passenger to the queue for a given floor.
		/// </summary>
		/// <param name="floor"></param>
		/// <param name="passengerDto"></param>
		public void AddPassengerToQueue(int floor, PassengerDTO passengerDto)
        {
            lock (_lock)
            {
                var passenger = _mapper.Map<PassengerDTO, Passenger>(passengerDto);

                if (!_floors.ContainsKey(floor))
                {
                    _floors[floor] = new Floor(floor);
                }
                _floors[floor].ScheduledPassengers.Enqueue(passenger);

                _passengerStatusSubject.OnNext(new PassengerEventArgs()
                {
                    CurrentFloor = floor,
                    ElevatorId = -1,
                    PassengerCount = -1,
                    PassengerStatus = PassengerEventArgs.Status.AddedToQueue
                });
            }
        }

}
}
