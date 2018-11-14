using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Tetris.GamePieces
{
	public enum ShapeFacing
	{
		TOP = 0,
		RIGHT = 1,
		BOTTOM = 2,
		LEFT = 3
	}

	public abstract class Shape
	{
		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Static Wallkick Tables ] ...>>>
		///////////////////////////////////////////////////

		// Only one of every shape will be instantiated, so using this method to reduce memory useage 
		// isnt super beneficial. However, this lends itself better to future extension or design change. 

		protected readonly static Dictionary<ShapeFacing, List<Point>> _wallKickData_Right = new Dictionary<ShapeFacing, List<Point>>
		{
			{ ShapeFacing.TOP, new List<Point> { new Point(0,0), new Point(-1,0), new Point(-1,-1), new Point(0,2), new Point(-1,2) } },
			{ ShapeFacing.RIGHT, new List<Point> { new Point(0,0), new Point(1,0), new Point(1,1), new Point(0,-2), new Point(1,-2) } },
			{ ShapeFacing.BOTTOM, new List<Point> { new Point(0,0), new Point(1,0), new Point(1,-1), new Point(0,2), new Point(1,2) } },
			{ ShapeFacing.LEFT, new List<Point> { new Point(0,0), new Point(-1,0), new Point(-1,1), new Point(0,-2), new Point(-1,-2) } }
		};

		protected readonly static Dictionary<ShapeFacing, List<Point>> _wallKickData_Left = new Dictionary<ShapeFacing, List<Point>>
		{
			{ ShapeFacing.RIGHT, new List<Point> { new Point(0,0), new Point(1,0), new Point(1,1), new Point(0,-2), new Point(1,-2) } },
			{ ShapeFacing.BOTTOM, new List<Point> { new Point(0,0), new Point(-1,0), new Point(-1,1), new Point(0,2), new Point(-1,2) } },
			{ ShapeFacing.LEFT, new List<Point> { new Point(0,0), new Point(-1,0), new Point(-1,1), new Point(0,-2), new Point(-1,-2) } },
			{ ShapeFacing.TOP, new List<Point> { new Point(0,0), new Point(1,0), new Point(1,-1), new Point(0,2), new Point(1,2) } }
		};

		protected readonly static Dictionary<ShapeFacing, List<Point>> _wallKickData_Right_I = new Dictionary<ShapeFacing, List<Point>>
		{
			{ ShapeFacing.TOP, new List<Point> { new Point(0,0), new Point(-2,0), new Point(1,0), new Point(-2,1), new Point(1,-2) } },
			{ ShapeFacing.RIGHT, new List<Point> { new Point(0,0), new Point(-1,0), new Point(2,0), new Point(-1,-2), new Point(2,1) } },
			{ ShapeFacing.BOTTOM, new List<Point> { new Point(0,0), new Point(2,0), new Point(-1,0), new Point(2,-1), new Point(-1,2) } },
			{ ShapeFacing.LEFT, new List<Point> { new Point(0,0), new Point(1,0), new Point(-2,0), new Point(1,2), new Point(-2,-1) } }
		};

		protected readonly static Dictionary<ShapeFacing, List<Point>> _wallKickData_Left_I = new Dictionary<ShapeFacing, List<Point>>
		{
			{ ShapeFacing.RIGHT, new List<Point> { new Point(0,0), new Point(2,0), new Point(-1,0), new Point(2,-1), new Point(-1,2) } },
			{ ShapeFacing.BOTTOM, new List<Point> { new Point(0,0), new Point(1,0), new Point(-2,0), new Point(1,2), new Point(-2,-1) } },
			{ ShapeFacing.LEFT, new List<Point> { new Point(0,0), new Point(-2,0), new Point(1,0), new Point(-2,1), new Point(1,-2) } },
			{ ShapeFacing.TOP, new List<Point> { new Point(0,0), new Point(-1,0), new Point(2,0), new Point(-1,-2), new Point(2,1) } }
		};

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Abstract Properties ] ...>>>
		///////////////////////////////////////////////////

		public abstract Rectangle SubTextureRect { get; }

		// Only one of every shape will be instantiated, so using this method to reduce memory usage 
		// isnt super beneficial. However, this lends itself better to future extension or design change. 
		protected abstract byte[ , ] _topLayout { get; }
		protected abstract byte[ , ] _rightLayout { get; }
		protected abstract byte[ , ] _bottomLayout { get; }
		protected abstract byte[ , ] _leftLayout { get; }

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Instance Properties ] ...>>>
		///////////////////////////////////////////////////

		public int X { get; set; }
		public int Y { get; set; }

		public ShapeFacing Facing { get; private set; }

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Spawn ] ...>>>
		///////////////////////////////////////////////////

		public void spawn( )
		{
			X = Board.WIDTH / 2 - 2;
			Y = 0;
			Facing = ShapeFacing.TOP;
		}

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Layout Retrieval] ...>>>
		///////////////////////////////////////////////////

		public byte[ , ] getLayout( Rotation rotation = Rotation.NONE )
		{
			switch( rotation )
			{
				case Rotation.NONE:
				default:
					return getNonRotatedLayout( );

				case Rotation.LEFT:
					return getRotatedLayout( Rotation.LEFT );

				case Rotation.RIGHT:
					return getRotatedLayout( Rotation.RIGHT );
			}
		}

		private byte[ , ] getNonRotatedLayout( )
		{
			switch( Facing )
			{
				case ShapeFacing.TOP: return _topLayout;
				case ShapeFacing.RIGHT: return _rightLayout;
				case ShapeFacing.BOTTOM: return _bottomLayout;
				case ShapeFacing.LEFT: return _leftLayout;
				default: throw new Exception( );
			}
		}

		private byte[ , ] getRotatedLayout( Rotation rotation )
		{
			switch( Facing )
			{
				case ShapeFacing.TOP: return rotation == Rotation.LEFT ? _leftLayout : _rightLayout;
				case ShapeFacing.RIGHT: return rotation == Rotation.LEFT ? _topLayout : _bottomLayout;
				case ShapeFacing.BOTTOM: return rotation == Rotation.LEFT ? _rightLayout : _leftLayout;
				case ShapeFacing.LEFT: return rotation == Rotation.LEFT ? _bottomLayout : _topLayout;
				default: throw new Exception( );
			}
		}

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Wallkick Data Retrieval  ] ...>>>
		///////////////////////////////////////////////////

		public virtual List<Point> getWallKickData( Rotation rotation )
		{
			switch( rotation )
			{
				case Rotation.LEFT:
					return _wallKickData_Left[ Facing ];

				case Rotation.RIGHT:
					return _wallKickData_Right[ Facing ];
				default: throw new Exception( );
			}
		}

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Rotation ] ...>>>
		///////////////////////////////////////////////////

		public void rotate( Rotation rotation )
		{
			switch( rotation )
			{
				case Rotation.LEFT:
					Facing--;
					if( Facing < ShapeFacing.TOP )
						Facing = ShapeFacing.LEFT;
					break;
				case Rotation.RIGHT:
					Facing++;
					if( Facing > ShapeFacing.LEFT )
						Facing = ShapeFacing.TOP;
					break;
				default: throw new Exception( );
			}
		}
	}
}
