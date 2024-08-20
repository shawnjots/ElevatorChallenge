using AutoMapper;
using ElevatorChallenge.DTOs.Passenger;
using ElevatorChallenge.Events;
using ElevatorChallenge.Models;
using ElevatorChallenge.Util;
using EventChallenge.Services.Interfaces;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace EventChallenge.Services
{
	public class ElevatorService : IElevatorService
	{
		private readonly IMapper? _mapper;
		private readonly object _lock = new();
		private readonly ConcurrentBag<Elevator>? _elevators;
		private readonly ConcurrentDictionary<int, Floor>? _floors;
		private readonly ConcurrentBag<Passenger>? _passengers;
		private readonly ConcurrentQueue<int>? _requestCount;


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
				
			});
		}

		private readonly Subject<PassengerEventArgs> _passengerStatusSubject = new();
		public IObservable<PassengerEventArgs> PassengerEvents => _passengerStatusSubject.AsObservable();
		
		private readonly Subject<ElevatorEventArgs> _elevatorStatusSubject = new();
		public IObservable<ElevatorEventArgs> ElevatorEvents => _elevatorStatusSubject.AsObservable();

		public void QueuePassenger(int floor, PassengerDTO passengerDTO)
		{
			throw new NotImplementedException();
		}

		private void EnableFloors()
		{
			for (int i = Constant.MinFloor; i <= Constant.MaxFloor; i++)
			{
				_floors[i] = new Floor(i);
			}
		}

		private void EnableElevators()
		{
			for (int i = 1; i <= Constant.MaxElevators; i++)
			{
				_elevators.Add(new Elevator(Constant.MaxPassengers));
			}
		}

		
	}
}
