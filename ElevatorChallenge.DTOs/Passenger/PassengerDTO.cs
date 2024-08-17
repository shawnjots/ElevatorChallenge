using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorChallenge.DTOs.Passenger
{
	public record PassengerDTO
	{
		public int CurrentFloor { get; set; }
		public int DestinationFloor { get; set; }
	}
}
