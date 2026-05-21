using UnityEngine;
using System;
using System.Collections;

/*
 * Defines the Game Info data structure.
 */

public class GameInfo
{

		// The two constants below are defined to aid readability:

		public const int _PLAYER1 = 0;
		public const int _PLAYER2 = 1;

		public enum GameStates
		{
				inGUI,
				playing_plr1,
				playing_plr2 }
		;

		private GameStates _state = GameStates.inGUI;

		public GameStates state {
				get {
						return _state;
				}
				set {
						_state = value;
				}
		}
	
		// I could have used an array for this, but it's Tic-Tac-Toe, not an RTS. 
		// Also, this avoids the readability problem caused by the C# array element counter starting at zero.
		Player player1 = new Player ();
		Player player2 = new Player ();

		// Constructor. Gets called when the class is instantiated and initialises the data we need.
		public GameInfo ()
		{

				// set default player info:
				player1.PlayerIs = beings.Human;
				player2.PlayerIs = beings.AI;

				// default player names:
				player1.PlayerName = "Player One";
				player2.PlayerName = "Player Two";

				// default pieces:
				player1.PlayingAs = pieces.O;
				player2.PlayingAs = pieces.X;

				// reset scores, wins, losses, etc.:
				player1.score = 0;
				player1.lost = 0;
				player1.won = 0;
				player1.drawn = 0;
				player1.totalGames = 0;

				player2.score = 0;
				player2.lost = 0;
				player2.won = 0;
				player2.drawn = 0;
				player2.totalGames = 0;


		}


		public Player GetPlayer (int playerNum)
		{
				// returns the selected player's state:
				switch (playerNum) {
				case _PLAYER1:
						return player1;

				case _PLAYER2:
						return player2;

				default:
						return null;
				}
		}

		public void StartPlaying ()
		{
				// Player 1 always plays first...
				state = GameStates.playing_plr1;
		}

		public Player GetCurrentPlayer ()
		{
				if (state == GameStates.playing_plr1)
						return player1;
				if (state == GameStates.playing_plr2)
						return player2;

				// This should only happen if the function is called and we're not playing a game.
				return null;
		}

		public pieces GetPlayerPiece (int playerNum)
		{

				pieces p = pieces.X;		// need to set it to something or the compiler will whinge about it being used before being assigned.

				switch (playerNum) {
				case _PLAYER1:
						p = player1.PlayingAs;
						break;
				case _PLAYER2:
						p = player2.PlayingAs;
						break;

				default:
			// should never get here!
						Debug.Log ("Invalid PlayerNum in GetPlayerPiece()");
						break;
				}

				return p;
		}

		public void SetPlayer (int playerNum, beings playeris, string playerName, pieces piece)
		{
				// update the relevant player's info:
				switch (playerNum) {
				case _PLAYER1:
						player1.PlayerIs = playeris;
						player1.PlayerName = playerName;
						player1.PlayingAs = piece;
						break;
		
				case _PLAYER2:
						player2.PlayerIs = playeris;
						player2.PlayerName = playerName;
						player2.PlayingAs = piece;
						break;

				// should never get here! This logs an error and tells us what "playerNum" was set to, to aid debugging.
				default:

						string errorString = String.Concat ("ERROR: SetPlayer passed invalid player number: ".ToString (), playerNum.ToString ());
						Debug.Log (errorString);

						break;

				}

		}

		private void UpdatePlayerScore (int playerNum, int score)
		{

				// update the relevant player's info:
				switch (playerNum) {
				case _PLAYER1:
						player1.score = score;
						break;
			
				case _PLAYER2:
						player2.score = score;
			
						break;
			
				// Logs an error and tells us what "playerNum" was set to.
				default:
			
						string errorString = String.Concat ("ERROR: SetPlayer passed invalid player number: ".ToString (), playerNum.ToString ());
						Debug.Log (errorString);
			
						break;
				}

		}

		public void GameWon (int playerNum)
		{
				switch (playerNum) {
				case _PLAYER1:
				// update win / loss stats, increment no. games played, then give winner 10 points:
						player1.won++;
						player1.totalGames++;
						UpdatePlayerScore (_PLAYER1, 10);
						player2.lost++;
						player2.totalGames++;	

						break;
				case _PLAYER2:
						player2.won++;
						player2.totalGames++;
						UpdatePlayerScore (_PLAYER2, 10);
						player1.lost++;
						player1.totalGames++;
						
						break;
				default:
						string errorString = String.Concat ("ERROR: SetPlayer passed invalid player number: ".ToString (), playerNum.ToString ());
						Debug.Log (errorString);
						
						break;
				}
		}

		public void GameDrawn ()
		{
				// Add one to 'drawn' stat...	increase total games played stat...	and add five points to each player's score for the draw
				player1.drawn++;
				player1.totalGames++;
				UpdatePlayerScore (_PLAYER1, 5);
				player2.drawn++;
				player2.totalGames++;
				UpdatePlayerScore (_PLAYER2, 5);

		}

}
