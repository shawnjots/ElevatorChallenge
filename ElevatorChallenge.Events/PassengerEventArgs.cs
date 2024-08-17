
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorChallenge.Events
{
	public class PassengerEventArgs
	{
		public int ElevatorId { get; set; }
		public int CurrentFloor { get; set; }
		public int PassengerCount { get; set; }
		public Status PassengerStatus { get; set; }

		public enum Status
		{
			AddedToQueue,
			BoardedElevator,
			DepartedElevator,
			RequestFailed
		}
	}
}
