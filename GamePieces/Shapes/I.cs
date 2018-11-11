using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Tetris.GamePieces
{
	class I : Shape
	{
		private static byte[ , ] _top = new byte[ , ] {
			{ 0, 0, 0, 0 },
			{ 1, 1, 1, 1 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 }
		};

		private static byte[ , ] _right = new byte[ , ] {
			{ 0, 0, 1, 0 },
			{ 0, 0, 1, 0 },
			{ 0, 0, 1, 0 },
			{ 0, 0, 1, 0 }
		};

		private static byte[ , ] _bottom = new byte[ , ] {
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 },
			{ 1, 1, 1, 1 },
			{ 0, 0, 0, 0 }
		};

		private static byte[ , ] _left = new byte[ , ] {
			{ 0, 1, 0, 0 },
			{ 0, 1, 0, 0 },
			{ 0, 1, 0, 0 },
			{ 0, 1, 0, 0 }
		};

		protected override byte[ , ] _topLayout => _top;
		protected override byte[ , ] _rightLayout => _right;
		protected override byte[ , ] _bottomLayout => _bottom;
		protected override byte[ , ] _leftLayout => _left;

		public override Rectangle SubTextureRect => new Rectangle( new Point( 32, 0 ), new Point( 128, 32 ) );

		// I uses a different wallkick table.
		public override List<Point> getWallKickData( Rotation rotation )
		{
			switch( rotation )
			{
				case Rotation.LEFT:
					return _wallKickData_Left_I[ Facing ];

				case Rotation.RIGHT:
					return _wallKickData_Right_I[ Facing ];
				default: throw new Exception( );
			}
		}

	}
}
