using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorChallenge.Models
{
	public class PathNode
	{
		public int Floor { get; set; }
		public int G { get; set; }
		public int H { get; set; }
		public int F { get; set; }
		public PathNode Parent { get; set; }

		public PathNode(int floor, int g, int h, PathNode parent)
		{
			Floor = floor;
			G = g;
			H = h;
			F = G + H;
			Parent = parent;
		}
	}
}