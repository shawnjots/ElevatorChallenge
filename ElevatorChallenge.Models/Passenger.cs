using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorChallenge.Models
{
	public class Passenger
	{
		public int Requestfloor { get; set; }
		public int DestinationFloor { get; set; }
		public DateTime RequestedAt { get; private set; }

		public Passenger(int requestFloor, int destinationFloor)
		{
			Requestfloor = requestFloor;
			DestinationFloor = destinationFloor;
			RequestedAt = DateTime.Now;
		}
	}
}