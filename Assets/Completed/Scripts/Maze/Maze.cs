using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AssemblyCSharp
{
	public static class Extensions
	{
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, System.Random rng)
		{
			var e = source.ToArray();
			for (var i = e.Length - 1; i >= 0; i--)
			{
				var swapIndex = rng.Next(i + 1);
				yield return e[swapIndex];
				e[swapIndex] = e[i];
			}
		}

		public static CellState OppositeWall(this CellState orig)
		{
			return (CellState)(((int) orig >> 2) | ((int) orig << 2)) & CellState.Initial;
		}

		public static bool HasFlag(this CellState cs,CellState flag)
		{
			return ((int)cs & (int)flag) != 0;
		}
	}

	[Flags]
	public enum CellState
	{
		Top = 1,
		Right = 2,
		Bottom = 4,
		Left = 8,
		Visited = 128,
		Initial = Top | Right | Bottom | Left,
	}


	public struct Point
	{
		public int x, y;
		public Point(int px, int py)
		{
			x = px;
			y = py;
		}
	}

	public struct RemoveWallAction
	{
		public AssemblyCSharp.Point Neighbour;
		public CellState Wall;
	}

	public class Maze
	{
		private readonly CellState[,] _cells;
		private readonly int _width;
		private readonly int _height;
		private readonly System.Random _rng;

		public Maze(int width, int height)
		{
			_width = width;
			_height = height;
			_cells = new CellState[width, height];
			for(var x=0; x<width; x++)
				for(var y=0; y<height; y++)
					_cells[x, y] = CellState.Initial;
			_rng = new System.Random();
			VisitCell(0, 0);
		}

		public CellState this[int x, int y]
		{
			get { return _cells[x,y]; }
			set { _cells[x,y] = value; }
		}

		public IEnumerable<RemoveWallAction> GetNeighbours(AssemblyCSharp.Point p)
		{
			if (p.x > 0) yield return new RemoveWallAction {Neighbour = new Point(p.x - 1, p.y), Wall = CellState.Left};
			if (p.y > 0) yield return new RemoveWallAction {Neighbour = new Point(p.x, p.y - 1), Wall = CellState.Top};
			if (p.x < _width-1) yield return new RemoveWallAction {Neighbour = new Point(p.x + 1, p.y), Wall = CellState.Right};
			if (p.y < _height-1) yield return new RemoveWallAction {Neighbour = new Point(p.x, p.y + 1), Wall = CellState.Bottom};
		}

		public void VisitCell(int x, int y)
		{
			this[x,y] |= CellState.Visited;
			foreach (var p in GetNeighbours(new Point(x, y)).Shuffle(_rng).Where(z => !(this[z.Neighbour.x, z.Neighbour.y].HasFlag(CellState.Visited))))
			{
				this[x, y] -= p.Wall;
				this[p.Neighbour.x, p.Neighbour.x] -= p.Wall.OppositeWall();
				VisitCell(p.Neighbour.x, p.Neighbour.y);
			}
		}

		public int[,] Display()
		{
			var maze = new int[_width*2, _height*2];

			for (var y = 0; y < _height; y++)
			{
				for (var x = 0; x < _width; x++)
				{
					if (this [x, y].HasFlag (CellState.Top)) {
						maze [x * 2, y * 2] = 1;
						maze [x * 2 + 1, y * 2] = 1;
					} else {
						maze [x * 2, y * 2] = 1;
						maze [x * 2 + 1, y * 2] = 0;
					}

					if (this [x, y].HasFlag (CellState.Left)) {
						maze [x * 2, y * 2 + 1] = 0;
						maze [x * 2 + 1, y * 2 + 1] = 0;
					} else {
						maze [x * 2, y * 2 + 1] = 1;
						maze [x * 2 + 1, y * 2 + 1] = 0;
					}
				}
			}
			return maze;
		}
	}
}

