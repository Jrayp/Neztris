using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Tetris.GamePieces
{
	class Z : Shape
	{
		private static byte[ , ] _top = new byte[ , ] {
			{ 7, 7, 0 },
			{ 0, 7, 7 },
			{ 0, 0, 0 },
		};

		private static byte[ , ] _right = new byte[ , ] {
			{ 0, 0, 7 },
			{ 0, 7, 7 },
			{ 0, 7, 0 },
		};

		private static byte[ , ] _bottom = new byte[ , ] {
			{ 0, 0, 0 },
			{ 7, 7, 0 },
			{ 0, 7, 7 },
		};

		private static byte[ , ] _left = new byte[ , ] {
			{ 0, 7, 0 },
			{ 7, 7, 0 },
			{ 7, 0, 0 },
		};

		protected override byte[ , ] _topLayout => _top;
		protected override byte[ , ] _rightLayout => _right;
		protected override byte[ , ] _bottomLayout => _bottom;
		protected override byte[ , ] _leftLayout => _left;

		public override Rectangle SubTextureRect => new Rectangle( new Point( 0, 160 ), new Point( 96, 64 ) );
	}
}
