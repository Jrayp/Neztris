using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Nez;
using Nez.Sprites;
using Tetris.GamePieces;

namespace Tetris
{
	public enum Sound
	{
		CLEAR_LINE,
		LAND,
		LVL_UP,
		MOVE,
		ROTATE,
		TETRIS,
		GAME_OVER
	}

	public class Game : Core
	{
		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Constants ] ...>>>
		///////////////////////////////////////////////////

		private const int TEXT_LINE_HEIGHT = 40;

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Audio ] ...>>>
		///////////////////////////////////////////////////

		private static Dictionary<Sound, SoundEffect> _sounds;

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Static Control Fields ] ...>>>
		///////////////////////////////////////////////////

		// Next Shape
		private static Texture2D _nextShapeSpriteSheet;
		private static Sprite _nextShapeSprite;
		// Score
		private static Text _scoreTxt;
		private static Text _linesTxt;
		private static Text _levelTxt;

		public Game( ) : base( width: 1024, height: 768, isFullScreen: false, enableEntitySystems: false, windowTitle: "Tetris", contentDirectory: "Content" )
		{ }

		protected override void Initialize( )
		{
			base.Initialize( );

			// Create the scene
			var playScene = Scene.createWithDefaultRenderer( new Color(51,68,85) );

			// Load the games fonts and create associated spritefonts
			var bigFont = playScene.content.Load<SpriteFont>("fonts/BigFont");
			var smallFont = playScene.content.Load<SpriteFont>("fonts/SmallFont");
			var bigSpritefont = new NezSpriteFont(bigFont);
			var smallSpritefont = new NezSpriteFont(smallFont);

			// Setup the scene
			var boardPos = setupBoard( playScene );
			setupScoreDisplay( playScene, bigSpritefont, boardPos );
			setupNextShapeDisplay( playScene, bigSpritefont, boardPos );
			setupHelp( playScene, smallSpritefont, boardPos );

			// Setup the audio
			setupAudio( playScene );

			// Set core scene
			scene = playScene;
		}

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ UI Layout Methods ] ...>>>
		///////////////////////////////////////////////////

		private Point setupBoard( Scene scene )
		{
			var boardEntity = new Board();
			scene.addEntity( boardEntity );

			var boardPos = new Point();
			boardPos.X = Window.ClientBounds.Width / 2 - boardEntity.PixelWidth / 2;
			boardPos.Y = Window.ClientBounds.Height / 2 - boardEntity.PixelHeight / 2;
			boardEntity.position = new Vector2( boardPos.X, boardPos.Y );

			return boardPos;
		}

		private void setupScoreDisplay( Scene scene, IFont spriteFont, Point boardPos )
		{
			var scoreEntity = scene.addEntity(new Entity("score"));

			var scoreLabel = new Text(spriteFont, "Score", new Vector2(0,0), Color.White);
			_scoreTxt = new Text( spriteFont, "000000", new Vector2( 0, TEXT_LINE_HEIGHT ), Color.White );
			scoreLabel.setHorizontalAlign( HorizontalAlign.Center );
			_scoreTxt.setHorizontalAlign( HorizontalAlign.Center );
			scoreEntity.addComponent( scoreLabel );
			scoreEntity.addComponent( _scoreTxt );

			var linesLabel = new Text(spriteFont, "Lines", new Vector2(0, TEXT_LINE_HEIGHT * 4), Color.White);
			_linesTxt = new Text( spriteFont, "0", new Vector2( 0, TEXT_LINE_HEIGHT * 5 ), Color.White );
			linesLabel.setHorizontalAlign( HorizontalAlign.Center );
			_linesTxt.setHorizontalAlign( HorizontalAlign.Center );
			scoreEntity.addComponent( linesLabel );
			scoreEntity.addComponent( _linesTxt );

			var levelLabel = new Text(spriteFont, "Level", new Vector2(0, TEXT_LINE_HEIGHT * 8), Color.White);
			_levelTxt = new Text( spriteFont, "0", new Vector2( 0, TEXT_LINE_HEIGHT * 9 ), Color.White );
			levelLabel.setHorizontalAlign( HorizontalAlign.Center );
			_levelTxt.setHorizontalAlign( HorizontalAlign.Center );
			scoreEntity.addComponent( levelLabel );
			scoreEntity.addComponent( _levelTxt );

			var scoreX =  boardPos.X / 2;
			var scoreY =  boardPos.Y + Board.TILESIZE;

			scoreEntity.position = new Vector2( scoreX, scoreY );
		}

		private void setupNextShapeDisplay( Scene scene, IFont spriteFont, Point boardPos )
		{
			var nextEntity = scene.addEntity(new Entity("next"));
			var nextLabel = new Text(spriteFont, "Next", new Vector2(0,0), Color.White);
			nextLabel.setHorizontalAlign( HorizontalAlign.Center );
			nextEntity.addComponent( nextLabel );

			_nextShapeSpriteSheet = scene.content.Load<Texture2D>( "images/shapes" );
			_nextShapeSprite = nextEntity.addComponent( new Sprite( ) );

			var spriteX = 0;
			var spriteY = TEXT_LINE_HEIGHT * 2;

			_nextShapeSprite.localOffset = new Vector2( spriteX, spriteY );

			var nextX =  Window.ClientBounds.Width - boardPos.X / 2;
			var nextY =  boardPos.Y + Board.TILESIZE;

			nextEntity.position = new Vector2( nextX, nextY );
		}

		private void setupHelp( Scene scene, IFont spriteFont, Point boardPos )
		{
			var leftHelpText =
			"Press:\n" +
			"Arrow keys to move.\n" +
			"'Z' to rotate left.\n" +
			"'X' to rotate right.\n" +
			"'L' to increase level.\n" +
			"'Q' to exit.";

			var helpEntityLeft = scene.addEntity(new Entity("left help"));
			var leftHelpLabel =  new Text(spriteFont, leftHelpText, new Vector2(0,0), Color.White);

			leftHelpLabel.setHorizontalAlign( HorizontalAlign.Center );
			helpEntityLeft.addComponent( leftHelpLabel );

			var helpX = Window.ClientBounds.Width - boardPos.X / 2;
			var helpY = boardPos.Y + Board.TILESIZE * 17;

			helpEntityLeft.position = new Vector2( helpX, helpY );
		}

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [ Static UI Control Methods ] ...>>>
		///////////////////////////////////////////////////

		public static void setNextSprite( Shape shape )
		{
			_nextShapeSprite.setSubtexture( new Nez.Textures.Subtexture( _nextShapeSpriteSheet, shape.SubTextureRect ) );
		}

		public static void setPoints( int points )
		{
			_scoreTxt.setText( points.ToString( ).PadLeft( 6, '0' ) );
		}

		public static void setLines( int lines )
		{
			_linesTxt.setText( lines.ToString( ) );
		}

		public static void setLevel( int level )
		{
			_levelTxt.setText( level.ToString( ) );
		}

		//////////////////////////////////////////////////////////////////////////////////////
		// <<<... [  Audio ] ...>>>
		///////////////////////////////////////////////////

		private void setupAudio( Scene scene )
		{
			_sounds = new Dictionary<Sound, SoundEffect>( );

			_sounds[ Sound.CLEAR_LINE ] = scene.content.Load<SoundEffect>( "sounds/Clear Line" );
			_sounds[ Sound.LAND ] = scene.content.Load<SoundEffect>( "sounds/Land" );
			_sounds[ Sound.LVL_UP ] = scene.content.Load<SoundEffect>( "sounds/Lvl Up 1" );
			_sounds[ Sound.MOVE ] = scene.content.Load<SoundEffect>( "sounds/Move" );
			_sounds[ Sound.ROTATE ] = scene.content.Load<SoundEffect>( "sounds/Rotate" );
			_sounds[ Sound.TETRIS ] = scene.content.Load<SoundEffect>( "sounds/Tetris" );
			_sounds[ Sound.GAME_OVER ] = scene.content.Load<SoundEffect>( "sounds/Game Over" );

			SoundEffect.MasterVolume = 1;
		}

		public static void playSound( Sound sound )
		{
			_sounds[ sound ].Play( );
		}

		public static void stopMusic( )
		{
			MediaPlayer.Stop( );
		}

		public static void startMusic( )
		{
			var music = scene.content.Load<Song>( "music/Music" );
			MediaPlayer.Play( music );
			MediaPlayer.Volume = 2;
			MediaPlayer.IsRepeating = true;
		}
	}
}
