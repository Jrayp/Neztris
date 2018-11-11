using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Tetris.GamePieces
{
    class O : Shape
    {
        private static byte[ , ] _layout = new byte[ , ] {
            { 0, 4, 4, 0 },
            { 0, 4, 4, 0 },
            { 0, 0, 0, 0 }
        };

		protected override byte[ , ] _topLayout => _layout;
		protected override byte[ , ] _rightLayout => _layout;
		protected override byte[ , ] _bottomLayout => _layout;
		protected override byte[ , ] _leftLayout => _layout;

		public override Rectangle SubTextureRect => new Rectangle( new Point( 128, 160 ), new Point( 64, 64 ) );

    }
}
