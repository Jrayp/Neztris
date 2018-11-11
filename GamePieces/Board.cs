using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Tiled;

namespace Tetris.GamePieces
{
	public enum BoardState
	{
		PLAY,
		ROW_CLEAR,
		CLEAR,
		REVEAL
	}

	public enum Transform
	{
		ROTATE_LEFT,
		ROTATE_RIGHT,
		LEFT,
		RIGHT,
		DOWN
	}

	public enum Rotation
	{
		LEFT,
		RIGHT,
		NONE
	}

	class Board : Entity
	{
		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Constants ] ...>>>
		///////////////////////////////////////////////////

		public const int WIDTH                 = 10; // Must be min 8 and even for systems to work.
		public const int HEIGHT                = 20;
		public const int TILESIZE              = 32;
		public const int TILE_COUNT            = 9;
		public const int TILESET_COLUMNS       = 9;
		public const int CLEAR_INTERVAL        = 4;
		public const float FIRST_SIDE_DELAY    = .175f;
		public const float SIDE_DELAY          = .04f;
		public const float FIRST_DOWN_DELAY    = .025f;
		public const float DOWN_DELAY          = .025f;

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Private Fields ] ...>>>
		///////////////////////////////////////////////////

		private BoardState _state;

		private TiledTile[ , ] _tiles;
		private TiledMap _tileMap;
		private Shape _currentShape;

		private int[] _rowSize;
		private SortedSet<int> _fullRows;

		private VirtualButton _leftInput;
		private VirtualButton _rightInput;
		private VirtualButton _downInput;
		private VirtualButton _rotateRightInput;
		private VirtualButton _rotateLeftInput;

		private int _clearTimer;
		private int _clearStep;
		private int _tickTimerMax;
		private int _tickTimer;

		private int _score;
		private int _lines;
		private int _level;

		private List<Shape> _shapeBag;
		private int _currentBagPos;

		private int _halfWidth = WIDTH / 2;

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Public Properties ] ...>>>
		///////////////////////////////////////////////////

		public int PixelWidth { get { return _tileMap.widthInPixels; } }
		public int PixelHeight { get { return _tileMap.heightInPixels; } }

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Constructor & Setup ] ...>>>
		///////////////////////////////////////////////////

		public Board( )
		{
			_state = BoardState.PLAY;
			_tiles = new TiledTile[ WIDTH, HEIGHT ];
			_tileMap = new TiledMap( 0, WIDTH, HEIGHT, TILESIZE, TILESIZE );
			_rowSize = new int[ HEIGHT ];
			_fullRows = new SortedSet<int>( );
			_shapeBag = new List<Shape> { new I( ), new J( ), new L( ), new O( ), new S( ), new T( ), new Z( ) };
			_clearTimer = CLEAR_INTERVAL;
			_clearStep = 0;
			_tickTimerMax = 60;
			_tickTimer = 60;
		}

		public override void onAddedToScene( )
		{
			base.onAddedToScene( );

			// Create and add the tiled map component
			addTiledMapComponent( );

			// Spawn the first shapes
			_shapeBag.shuffle( );
			spawnShape( );

			// Setup input
			setupInput( );

			// Start the music
			Game.startMusic( );
		}

		private void addTiledMapComponent( )
		{
			var spriteSheet = scene.content.Load<Texture2D>( "images/blocks" );
			var tileset = _tileMap.createTileset( spriteSheet, 0, TILESIZE, TILESIZE, true, 0, 0, TILE_COUNT, TILESET_COLUMNS );

			// Nez' tiledmap implementation will only take 1d arrays, 
			// so we have to do this if we want to use the more intuitive
			// 2d array, unfortunatly.
			var _tilesTEMP = new TiledTile[WIDTH*HEIGHT];
			for( int x = 0; x < _tiles.GetLength( 0 ); x++ )
				for( int y = 0; y < _tiles.GetLength( 1 ); y++ )
				{
					_tiles[ x, y ] = new TiledTile( 0 );
					_tiles[ x, y ].tileset = tileset;
					_tilesTEMP[ WIDTH * y + x ] = _tiles[ x, y ];
				}

			var layer = _tileMap.createTileLayer( "Default", WIDTH, HEIGHT, _tilesTEMP );
			var component = new TiledMapComponent( _tileMap );

			addComponent( component );
		}

		private void setupInput( )
		{
			_leftInput = new VirtualButton( );
			_leftInput.nodes.Add( new VirtualButton.KeyboardKey( Keys.Left ) );
			_leftInput.nodes.Add( new VirtualButton.GamePadButton( 0, Buttons.DPadLeft ) );
			_leftInput.setRepeat( FIRST_SIDE_DELAY, SIDE_DELAY );

			_rightInput = new VirtualButton( );
			_rightInput.nodes.Add( new VirtualButton.KeyboardKey( Keys.Right ) );
			_rightInput.nodes.Add( new VirtualButton.GamePadButton( 0, Buttons.DPadRight ) );
			_rightInput.setRepeat( FIRST_SIDE_DELAY, SIDE_DELAY );

			_downInput = new VirtualButton( );
			_downInput.nodes.Add( new VirtualButton.KeyboardKey( Keys.Down ) );
			_downInput.nodes.Add( new VirtualButton.GamePadButton( 0, Buttons.DPadDown ) );
			_downInput.setRepeat( FIRST_DOWN_DELAY, DOWN_DELAY );

			_rotateLeftInput = new VirtualButton( );
			_rotateLeftInput.nodes.Add( new VirtualButton.KeyboardKey( Keys.Z ) );
			_rotateLeftInput.nodes.Add( new VirtualButton.GamePadButton( 0, Buttons.A ) );

			_rotateRightInput = new VirtualButton( );
			_rotateRightInput.nodes.Add( new VirtualButton.KeyboardKey( Keys.X ) );
			_rotateRightInput.nodes.Add( new VirtualButton.GamePadButton( 0, Buttons.B ) );
		}

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Update Methods ] ...>>>
		///////////////////////////////////////////////////

		public override void update( )
		{
			base.update( );

			switch( _state )
			{
				case BoardState.PLAY:
					handleTicks( );
					handleInput( );
					break;

				case BoardState.ROW_CLEAR:
					handleRowClear( );
					break;

				case BoardState.CLEAR:
					clearBoard( );
					break;

				case BoardState.REVEAL:
					revealBoard( );
					break;
			}
		}

		private void handleTicks( )
		{
			if( _tickTimer > 0 )
				_tickTimer--;
			else
			{
				_tickTimer = _tickTimerMax;
				tick( );
			}
		}

		private void handleInput( )
		{
			// Left & Right
			if( _leftInput.isPressed )
				tryTransform( Transform.LEFT );
			else if( _rightInput.isPressed )
				tryTransform( Transform.RIGHT );

			// Rotation
			if( _rotateLeftInput.isPressed )
				tryTransform( Transform.ROTATE_LEFT );
			else if( _rotateRightInput.isPressed )
				tryTransform( Transform.ROTATE_RIGHT );

			// Down
			if( _downInput.isPressed )
				tick( true );

			// Misc 
			if( Input.isKeyPressed( Keys.L ) )
				increaseLevel( );

			if( Input.isKeyPressed( Keys.Q ) )
				Environment.Exit( 1 );
		}

		private void tick( bool softDrop = false )
		{
			tryTransform( Transform.DOWN );

			if( softDrop )
				addPoints( 1 );

			_tickTimer = _tickTimerMax;
		}

		private void handleRowClear( )
		{
			_clearTimer--;
			if( _clearTimer == 0 )
			{
				clearRows( _clearStep );
				_clearTimer = CLEAR_INTERVAL;
				_clearStep++;
			}
			if( _clearStep == WIDTH / 2 )
			{
				_clearTimer = CLEAR_INTERVAL;
				_clearStep = 0;
				shiftRows( );
				spawnShape( );
				_state = BoardState.PLAY;
			}
		}

		private void clearBoard( )
		{
			_clearTimer--;
			if( _clearTimer == 0 )
			{
				for( int x = 0; x < WIDTH; x++ )
					_tiles[ x, HEIGHT - 1 - _clearStep ].id = 8;

				_clearTimer = CLEAR_INTERVAL;
				_clearStep++;
			}

			if( _clearStep == HEIGHT )
				resetBoard( );
		}

		private void resetBoard( )
		{
			_rowSize = new int[ HEIGHT ];

			_level = 0;

			Game.setPoints( 0 );
			Game.setLines( 0 );
			Game.setLevel( 0 );

			_state = BoardState.REVEAL;
		}

		private void revealBoard( )
		{
			_clearTimer--;
			if( _clearTimer == 0 )
			{
				for( int x = 0; x < WIDTH; x++ )
					_tiles[ x, HEIGHT - _clearStep ].id = 0;

				_clearTimer = CLEAR_INTERVAL;
				_clearStep--;
			}

			if( _clearStep == 0 )
			{
				_shapeBag.shuffle( );
				_currentBagPos = 0;
				spawnShape( );
				Game.startMusic( );
				_state = BoardState.PLAY;
			}
		}

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Shape Transforming ] ...>>>
		///////////////////////////////////////////////////

		private void tryTransform( Transform transform )
		{
			drawShape( false );

			switch( transform )
			{
				case Transform.ROTATE_LEFT:
					if( tryRotateShape( Rotation.LEFT ) )
						Game.playSound( Sound.ROTATE );
					break;

				case Transform.ROTATE_RIGHT:
					if( tryRotateShape( Rotation.RIGHT ) )
						Game.playSound( Sound.ROTATE );
					break;

				case Transform.LEFT:
					if( !checkCollision( -1, 0, Rotation.NONE ) )
					{
						_currentShape.X--;
						Game.playSound( Sound.MOVE );
					}
					break;

				case Transform.RIGHT:
					if( !checkCollision( 1, 0, Rotation.NONE ) )
					{
						_currentShape.X++;
						Game.playSound( Sound.MOVE );
					}
					break;

				case Transform.DOWN:
					if( checkLanded( ) )
					{
						drawShape( true, true );
						Game.playSound( Sound.LAND );
						if( _fullRows.Count == 0 )
						{
							spawnShape( );
							return;
						}
						else
						{
							if( _fullRows.Count == 4 )
								Game.playSound( Sound.TETRIS );
							else
								Game.playSound( Sound.CLEAR_LINE );

							_state = BoardState.ROW_CLEAR;
						}
					}
					else
						_currentShape.Y++;
					break;
			}
			drawShape( true );
		}

		private void drawShape( bool draw, bool landed = false )
		{
			var layout = _currentShape.getLayout( );
			var width = layout.GetLength( 1 );
			var height = layout.GetLength( 0 );

			for( int x = 0; x < width; x++ )
				for( int y = 0; y < height; y++ )
				{
					if( layout[ y, x ] > 0 )
					{
						var boardX = x + _currentShape.X;
						var boardY = y + _currentShape.Y;

						var value = draw ? layout[ y, x ] : 0;
						_tiles[ boardX, boardY ].id = value;

						if( landed )
							addToRow( boardY );
					}
				}
		}

		private bool tryRotateShape( Rotation rotation )
		{
			var wallKickData = _currentShape.getWallKickData( rotation );

			foreach( Point shift in wallKickData )
			{
				if( !checkCollision( shift.X, shift.Y, rotation ) )
				{
					_currentShape.X += shift.X;
					_currentShape.Y += shift.Y;
					_currentShape.rotate( rotation );
					return true;
				}
			}
			return false;
		}

		private bool checkCollision( int xShift, int yShift, Rotation rotation )
		{
			var layout = _currentShape.getLayout( rotation );
			var width = layout.GetLength( 1 );
			var height = layout.GetLength( 0 );

			for( int x = 0; x < width; x++ )
				for( int y = 0; y < height; y++ )
				{
					var boardX = x + _currentShape.X + xShift;
					var boardY = y + _currentShape.Y + yShift;

					if( layout[ y, x ] > 0 && ( !inBounds( boardX, boardY ) || _tiles[ boardX, boardY ].id > 0 ) )
						return true;
				}
			return false;
		}

		private bool checkLanded( )
		{
			var layout = _currentShape.getLayout( );
			var width = layout.GetLength( 1 );
			var height = layout.GetLength( 0 );

			for( int x = 0; x < width; x++ )
				for( int y = 0; y < height; y++ )
				{
					if( layout[ y, x ] > 0 )
					{
						var boardX = x + _currentShape.X;
						var boardY = y + _currentShape.Y;

						if( boardY + 1 >= HEIGHT || _tiles[ boardX, boardY + 1 ].id > 0 )
							return true;
					}
				}
			return false;
		}


		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Row Clearing ] ...>>>
		///////////////////////////////////////////////////

		private void addToRow( int row )
		{
			_rowSize[ row ]++;
			if( _rowSize[ row ] == WIDTH )
				_fullRows.Add( row );
		}

		private void clearRows( int step )
		{
			foreach( int row in _fullRows )
			{
				#region Clear line Debugging
#if DEBUG
				// Test 1
				if( step == 0 )
				{
					var counter = 0;
					for( int x = 0; x < WIDTH; x++ )
						if( _tiles[ x, row ].id > 0 )
							counter++;

					if( counter < WIDTH )
						throw new Exception( "Nr: " + counter );
				}

				// TEST 2
				if( _tiles[ _halfWidth - 1 - step, row ].id == 0 || _tiles[ _halfWidth + step, row ].id == 0 )
					throw new Exception( "Not full!" );
#endif
				#endregion

				_tiles[ _halfWidth - 1 - step, row ].id = 0;
				_tiles[ _halfWidth + step, row ].id = 0;
			}
		}

		private void shiftRows( )
		{
			// This could be optimized fairly significantly I think,
			// however this is easier to read, less bug prone,
			// and isnt causing any performance issues, so eh.

			foreach( int row in _fullRows )
				for( int y = row; y > 0; y-- )
					for( int x = 0; x < WIDTH; x++ )
						_tiles[ x, y ].id = _tiles[ x, y - 1 ].id;

			for( int y = 0; y < HEIGHT; y++ )
			{
				_rowSize[ y ] = 0;
				for( int x = 0; x < WIDTH; x++ )
					if( _tiles[ x, y ].id > 0 )
						_rowSize[ y ]++;
			}

			scoreLineClear( _fullRows.Count );
			_fullRows.Clear( );
		}

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Shape Spawning ] ...>>>
		///////////////////////////////////////////////////

		private void spawnShape( )
		{
			_currentShape = _shapeBag[ _currentBagPos ];
			_currentShape.spawn( );


			if( checkCollision( 0, 0, Rotation.NONE ) )
				gameOver( );

			drawShape( true );

			_currentBagPos++;
			if( _currentBagPos == 7 )
			{
				_shapeBag.shuffle( );
				_currentBagPos = 0;
			}

			Game.setNextSprite( _shapeBag[ _currentBagPos ] );
		}

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Scoring & Game Over ] ...>>>
		///////////////////////////////////////////////////

		public void addPoints( int p )
		{
			_score += p;

			if( _score > 999999 )
				_score = 999999;

			Game.setPoints( _score );
		}

		private void scoreLineClear( int nrLines )
		{
			switch( nrLines )
			{
				case 1: addPoints( 40 * ( _level + 1 ) ); break;
				case 2: addPoints( 100 * ( _level + 1 ) ); break;
				case 3: addPoints( 300 * ( _level + 1 ) ); break;
				case 4: addPoints( 1200 * ( _level + 1 ) ); break;
				default: throw new Exception( );
			}

			increaseLines( nrLines );
		}

		private void increaseLines( int nrLines )
		{
			_lines += nrLines;
			Game.setLines( _lines );

			if( _lines >= ( _level + 1 ) * 10 )
				increaseLevel( );
		}

		private void increaseLevel( )
		{
			_level++;
			_tickTimerMax = Mathf.roundToInt( Mathf.pow( .8f - ( _level * .007f ), _level ) * 60 );
			Game.playSound( Sound.LVL_UP );
			Game.setLevel( _level );
		}

		private void gameOver( )
		{
			Game.playSound( Sound.GAME_OVER );
			Game.stopMusic( );
			_state = BoardState.CLEAR;
		}

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Utility ] ...>>>
		///////////////////////////////////////////////////

		private bool inBounds( int x, int y )
		{
			if( x >= 0 && x < WIDTH && y >= 0 && y < HEIGHT )
				return true;
			else
				return false;
		}
	}

}
