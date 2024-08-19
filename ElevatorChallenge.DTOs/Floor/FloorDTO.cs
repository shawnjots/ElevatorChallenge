using ElevatorChallenge.DTOs.Passenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorChallenge.DTOs.Floor
{
	public record FloorDTO
	{
		public int FloorNumber { get; set; }
		public List<PassengerDTO>? PassengerQueue { get; set; }
	}
}
