using ElevatorChallenge.DTOs.Passenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorChallenge.DTOs.Elevator
{
	public record ElevatorDTO
	{
		public int ID { get; set; }
		public int CurrentFloor { get; set; }
		public List<PassengerDTO>? Passengers { get; set; }
		public ElevatorStatusDTO? Status { get; set; }
		public ElevatorDirectionDTO? Direction { get; set; }
	}
}
