﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Lidgren.Network;
using TOJAM12.Entities;
using System.IO;

namespace TOJAM12
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class TojamGame : Game
	{
		public GraphicsDeviceManager graphics;
		public SpriteBatch spriteBatch;
        public SpriteFont GameFont;
		public Config config;

		// declaration of scenes in the game
		Scene[] scenes = {
			new ChatScene(),
			new PlayerSelectScene(),
			new GameScene(),
		};
		public enum GameScenes
		{
			Chat = 0,
			PlayerSelect = 1,
			Game = 2,
		};

        public Scene GetScene(GameScenes scene)
        {
            return scenes[(int)scene];
        }

		GameScenes activeSceneType = GameScenes.PlayerSelect;
		Scene activeScene = null;
        public GameInstance gameInstance;

        public static Song song;
        public static bool resetKey = false;

		public TojamGame()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
			graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
			graphics.IsFullScreen = true;
			Content.RootDirectory = "Content";
			activeScene = scenes[(int) activeSceneType];

			// read config file from working directory
			Console.WriteLine(Directory.GetCurrentDirectory());
			config = Config.ReadFromFile("config.txt");
		}

		public void SwitchScene(GameScenes newSceneType, Dictionary<string, object> sceneParameters = null)
		{
			Debug.WriteLine("switch scene to " + newSceneType);
			this.activeSceneType = newSceneType;
			Scene nextScene = scenes[(int)activeSceneType];
			nextScene.onTransition(sceneParameters);
			activeScene = nextScene;
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()

		{
			base.Initialize();
			foreach (Scene s in scenes)
			{
				s.Initialize(this);
			}
            gameInstance = new GameInstance(this);
		}

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
		{
            
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
			GameFont = Content.Load<SpriteFont>("fonts/Cutive_Mono");

            // song = Content.Load<Song>("sounds/ontheroad");

            //MediaPlayer.Play(song);

			foreach (Scene s in scenes)
			{
				s.LoadContent(this);
			}

			// for debug purposes, jump immediately into a 2p game
			//Dictionary<str====ing, object> parameters = new Dictionary<string, object>();
			//parameters["player1"] = new Input(Input.Type.Keyboard);
			//parameters["player2"] = new Input(Input.Type.JoypadOne);
			//this.SwitchScene(GameScenes.Game, parameters);
			this.SwitchScene(GameScenes.Chat);

		}

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // For Mobile devices, this logic will close the Game when the Back button is pressed
            // Exit() is obsolete on iOS
#if !__IOS__ && !__TVOS__
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
#endif

            if (!resetKey && Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                foreach (Keys key in Keyboard.GetState().GetPressedKeys())
                {
                    if (key == Keys.Tab)
                    {
                        if (graphics.IsFullScreen) graphics.IsFullScreen = false;
                        else graphics.IsFullScreen = true;
                        resetKey = true;
                        graphics.ApplyChanges();
                    }
                }
            }

            if (resetKey) { if (Keyboard.GetState().IsKeyUp(Keys.Tab)) resetKey = false; }

            foreach (Input i in Input.getAllInstances())
            {
                i.Update();
            }

            activeScene.Update(this, gameTime);
            gameInstance.Update(gameTime);

            base.Update(gameTime);
        }

        
		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

			activeScene.Draw(this, gameTime);

			base.Draw(gameTime);
		}

        internal static void StartMusic()
        {
            if(MediaPlayer.State != MediaState.Playing) MediaPlayer.Play(song);
        }
        internal static void StopMusic()
        {
            if (MediaPlayer.State == MediaState.Playing) MediaPlayer.Stop();
        }
    }
}
