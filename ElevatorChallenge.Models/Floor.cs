using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorChallenge.Models
{
	public class Floor
	{
		public int FloorLevel { get; private set; }
		public ConcurrentQueue<Passenger> ScheduledPassengers { get; set; }


		public Floor(int floor) 
		{
			FloorLevel = floor;
			ScheduledPassengers = new ConcurrentQueue<Passenger>();
		}

		
	}
}