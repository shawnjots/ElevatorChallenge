using ElevatorChallenge.DTOs.Passenger;
using ElevatorChallenge.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventChallenge.Services.Interfaces
{
	public interface IElevatorService
	{
		IObservable<PassengerEventArgs> PassengerEvents { get; }
		IObservable<ElevatorEventArgs> ElevatorEvents { get; }

		void QueuePassenger(int floor, PassengerDTO passengerDTO);
	}
}
