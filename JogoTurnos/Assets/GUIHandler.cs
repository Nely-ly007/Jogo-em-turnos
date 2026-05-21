using UnityEngine;
using System.Collections;


public class GUIHandler : MonoBehaviour
{

		// point these at the sprites used in the GUI...

		// In this release, the GUI is created entirely using 2D sprites with 2D box colliders.

		public Transform button_PLAY;
		public Transform button_RESIGN;

		public Transform player1_Label;
		public Transform player1_Computer;
		public Transform player1_Human;
		public Transform player1_X;
		public Transform player1_O;

		public Transform player2_Label;
		public Transform player2_Computer;
		public Transform player2_Human;
		public Transform player2_X;
		public Transform player2_O;
		
		// the animated game-over graphics:
		public Transform txt_Draw;
		public Transform txt_Plr1Wins;
		public Transform txt_Plr2Wins;



		private Camera mainCamera;

		private GameInfo gInfo;

		private BoardManager board;

		private AIPlayer aiPlayer;
		
		private bool suspendInteractions = false;		// set to true during game-over animations to prevent access to buttons.


		// Use this for initialization
		void Start ()
		{

				gInfo = new GameInfo ();
				
				// sanity-check gInfo...
				if (gInfo == null) {
						Debug.Log ("ERROR: Could not initialise gInfo!");
				}
		
		
				mainCamera = Camera.main;
				if (!mainCamera) {
						Debug.Log ("Error: Unable to find MainCamera");
				}

				board = FindObjectOfType <BoardManager> ();
				aiPlayer = FindObjectOfType <AIPlayer> ();

			
				// Initialise the players:
				gInfo.SetPlayer (GameInfo._PLAYER1, beings.Human, "Player 1", pieces.X);
				gInfo.SetPlayer (GameInfo._PLAYER2, beings.AI, "Player 2", pieces.O);

				// sanity-check for the sprite list...

				if (!(button_PLAY && button_RESIGN && player1_Computer && player1_Human && player1_X && player1_O && player2_Computer && player2_Human && player2_X && player2_O && txt_Draw && txt_Plr1Wins && txt_Plr2Wins)) {
						Debug.Log ("GUI Transforms missing! Did you forget to add them in the Editor?");
				}
				
				// Make sure the various game-over messages are hidden too...
				
				txt_Plr1Wins.gameObject.SetActive (false);
				txt_Plr2Wins.gameObject.SetActive (false);
				txt_Draw.gameObject.SetActive (false);


		}

		// Get the current player's piece and return an int containing EMPTYCELL, O_CELL or X_CELL, suitable for BoardManager.
		public int GetCurrentPlayerPieceAsInt ()
		{
				// returns the current player.
				pieces piece = gInfo.GetCurrentPlayer ().PlayingAs;
			
				int result = BoardManager.EMPTYCELL;				// return this if there's an invalid value.
				if (piece == pieces.O)
						result = BoardManager.O_CELL;
				if (piece == pieces.X)
						result = BoardManager.X_CELL;
				
				return result;
			
		}
		// Called by BoardManager to signal when the game has been won.
		public void GameWon ()
		{
				int playerNum = 0;
				bool didXWin = board.WasWinnerX ();
				
				pieces p1Piece = gInfo.GetPlayerPiece (GameInfo._PLAYER1);
				
				// this is all we need to know who won.

				if (didXWin) {
						// find out who was playing as "X"...
						playerNum = (p1Piece == pieces.X) ? GameInfo._PLAYER1 : GameInfo._PLAYER2;
				} else {
						// looks like "O" won...
						playerNum = (p1Piece == pieces.O) ? GameInfo._PLAYER1 : GameInfo._PLAYER2;
				}
					
				
				if (playerNum == GameInfo._PLAYER1) {
				
						StartCoroutine (DoPlayer1WinsAnim ());
				}
				if (playerNum == GameInfo._PLAYER2) {
						StartCoroutine (DoPlayer2WinsAnim ());
					
				}
				gInfo.GameWon (playerNum);
				gInfo.state = GameInfo.GameStates.inGUI;
		

		}
	
		public void GameDrawn ()
		{
			
				gInfo.GameDrawn ();
			
				Debug.Log ("GAME DRAWN!");
				// TO-DO: This is where the fancy animation goes.
				StartCoroutine (DoItsADrawAnim ());		
			
				gInfo.state = GameInfo.GameStates.inGUI;
			
		}

		// The animations are triggered automatically in a looping mode.
		// It's not going to win a BAFTA, but it'll do for a tutorial.
		private IEnumerator DoPlayer1WinsAnim ()
		{
				suspendInteractions = true;		// disable all GUI activities while animation runs.
				txt_Plr1Wins.gameObject.SetActive (true);
				yield return new WaitForSeconds (3.0f);
				txt_Plr1Wins.gameObject.SetActive (false);
				suspendInteractions = false;		// Animation is done; re-enable the GUI.
		}

		private IEnumerator DoPlayer2WinsAnim ()
		{
				suspendInteractions = true;		// disable all GUI activities while animation runs.
				txt_Plr2Wins.gameObject.SetActive (true);
				yield return new WaitForSeconds (3.0f);
				txt_Plr2Wins.gameObject.SetActive (false);
				suspendInteractions = false;		// Animation is done; re-enable the GUI.
		}
	
		private IEnumerator DoItsADrawAnim ()
		{
				suspendInteractions = true;		// disable all GUI activities while animation runs.
				txt_Draw.gameObject.SetActive (true);
				yield return new WaitForSeconds (3.0f);
				txt_Draw.gameObject.SetActive (false);
				suspendInteractions = false;		// Animation is done; re-enable the GUI.
		}
	



		public void NextTurn ()
		{
				// swap current player over.
				if (gInfo.state != GameInfo.GameStates.inGUI)
						gInfo.state = (gInfo.state == GameInfo.GameStates.playing_plr1) ? GameInfo.GameStates.playing_plr2 : GameInfo.GameStates.playing_plr1;
			
				CheckAndPlayAI ();
		}
	
		private void CheckAndPlayAI ()
		{
				// Check if it's an AI's turn. If so, tell it the AI script to get on with it...
				if (gInfo.state != GameInfo.GameStates.inGUI) {
						if (gInfo.GetCurrentPlayer ().PlayerIs == beings.AI) {
								aiPlayer.PlayAIMove (gInfo.GetCurrentPlayer ().PlayingAs);
						}
				}			
		}
		
		
		// LateUpdate is called once per frame, after everything has been rendered.
		void LateUpdate ()
		{
				// This test suspends all GUI activity if a game-over animation is running.
				if (!suspendInteractions) {

						if (!(gInfo.state == GameInfo.GameStates.inGUI)) {
								button_RESIGN.gameObject.SetActive (true);
								button_PLAY.gameObject.SetActive (false);

								// Only a minimal GUI is shown in play: 
								// We show only the current player's label and piece ("O" or "X") to show whose turn it is.
								// Everything else is hidden...

								if (gInfo.state == GameInfo.GameStates.playing_plr1) {
										player1_Label.gameObject.SetActive (true);
										player1_O.gameObject.SetActive ((gInfo.GetPlayerPiece (GameInfo._PLAYER1) == pieces.O) ? true : false);
										player1_X.gameObject.SetActive ((gInfo.GetPlayerPiece (GameInfo._PLAYER1) == pieces.X) ? true : false);

										player2_Label.gameObject.SetActive (false);
										player2_O.gameObject.SetActive (false);
										player2_X.gameObject.SetActive (false);
								} else { // must be Player 2's turn...
										player2_Label.gameObject.SetActive (true);
										player2_O.gameObject.SetActive ((gInfo.GetPlayerPiece (GameInfo._PLAYER2) == pieces.O) ? true : false);
										player2_X.gameObject.SetActive ((gInfo.GetPlayerPiece (GameInfo._PLAYER2) == pieces.X) ? true : false);

										player1_Label.gameObject.SetActive (false);
										player1_O.gameObject.SetActive (false);
										player1_X.gameObject.SetActive (false);

								}

						} 
				
						if (gInfo.state == GameInfo.GameStates.inGUI) {

								// As we're not playing a game, make sure both player labels are shown:

								player1_Label.gameObject.SetActive (true);
								player2_Label.gameObject.SetActive (true);


								// Now update the GUI according to the content of gInfo...

								Player p = gInfo.GetPlayer (GameInfo._PLAYER1);

								// This next test doesn't trigger an error. Its purpose is to prevent a crash.
								if (p != null) {
										player1_Computer.gameObject.SetActive (p.PlayerIs == beings.AI);
										player1_Human.gameObject.SetActive (p.PlayerIs == beings.Human);
										player1_O.gameObject.SetActive (p.PlayingAs == pieces.O);
										player1_X.gameObject.SetActive (p.PlayingAs == pieces.X);
								}

								// ditto for Player 2...

								p = gInfo.GetPlayer (GameInfo._PLAYER2);
			
								if (p != null) {
										player2_Computer.gameObject.SetActive (p.PlayerIs == beings.AI);
										player2_Human.gameObject.SetActive (p.PlayerIs == beings.Human);
										player2_O.gameObject.SetActive (p.PlayingAs == pieces.O);
										player2_X.gameObject.SetActive (p.PlayingAs == pieces.X);
								}

								// set buttons at top...

								button_RESIGN.gameObject.SetActive (false);
								button_PLAY.gameObject.SetActive (true);

						}

						// What follows is very, very messy.
						// Once the new Unity GUI system is released, all you see here will likely be replaced.

						// Basically, when the user clicks on anything in our fake Sprite-based GUI,
						// we simply grab the Collider's tag to find out what they clicked on and act accordingly.

						// Note that we use "GetButtonUp" here.

						/*
				 * The "ButtonUp" state is only sent when a key or button has been released.
				 * 
				 * GetButtonDown could be used too, but waiting for the button to be released makes it less likely that the player will accidentally toggle an element twice in quick succession.
				 * GetButtonDown will always return true if a button is pressed and would result in our toggling the GUI elements very rapidly until the button is released. 
				 * I'd need to include a latching mechanism to prevent this – a process known as 'debouncing' – which is a process a proper GUI system would take care of for me.
				 * 
				 */



						if (Input.GetButtonUp ("Fire1")) {
								Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
								RaycastHit2D hit = Physics2D.GetRayIntersection (ray);
								if (hit) {
										string t = hit.collider.tag;

										Player p1 = gInfo.GetPlayer (GameInfo._PLAYER1);	// we'll need this info shortly.
										Player p2 = gInfo.GetPlayer (GameInfo._PLAYER2);

										if (!(gInfo.state == GameInfo.GameStates.inGUI)) {
												if (t == "btn_RESIGN") {	// this is the only working button in the GUI when playing.
														gInfo.state = GameInfo.GameStates.inGUI;
												} else if (t == "gridCell") {	// clicked on a board cell.
														// the board cells should only be clickable if the current player is a human.
														if (gInfo.GetCurrentPlayer ().PlayerIs == beings.Human) {
																GridCellScript gcScript = hit.collider.gameObject.GetComponent<GridCellScript> ();

																if (gcScript.state == GridCellScript.CellStates.empty) {
																		// we know we're playing a game, the target cell is empty, and the current player is a human, so work out whose turn it is and put their symbol on the cell.
																		if (gInfo.GetCurrentPlayer ().PlayingAs == pieces.O) {
																				gcScript.state = GridCellScript.CellStates.O;
																		} else
																				gcScript.state = GridCellScript.CellStates.X;

																		NextTurn ();	// Swap player.
																}
														}
												}

										} else { 		// (i.e. not playing a game)
												// these are all simple toggles, so clicking on "Computer" will set the player to "Human", 
												// clicking on "X" sets the piece to "O", etc.

												if (t == "player1_Computer") {
														// set to "Human" player.
														gInfo.SetPlayer (GameInfo._PLAYER1, beings.Human, p1.PlayerName, p1.PlayingAs);

												}

												// second verse, same as the first...
												if (t == "player1_Human") {
														// set to "Computer" player.
														gInfo.SetPlayer (GameInfo._PLAYER1, beings.AI, p1.PlayerName, p1.PlayingAs);
												}

												// The rest is much the same...

												if (t == "player1_O") {
														// set to "X" piece. This means toggling Player 2's piece too.
														gInfo.SetPlayer (GameInfo._PLAYER1, p1.PlayerIs, p1.PlayerName, pieces.X);
														gInfo.SetPlayer (GameInfo._PLAYER2, p2.PlayerIs, p2.PlayerName, pieces.O);

												}

												if (t == "player1_X") {
														// set to "O" piece...
														gInfo.SetPlayer (GameInfo._PLAYER1, p1.PlayerIs, p1.PlayerName, pieces.O);
														gInfo.SetPlayer (GameInfo._PLAYER2, p2.PlayerIs, p2.PlayerName, pieces.X);

												}

												// And now, the same trick again, but for Player 2...0

												if (t == "player2_Computer") {
														// set to "Human" player.
														gInfo.SetPlayer (GameInfo._PLAYER2, beings.Human, p2.PlayerName, p2.PlayingAs);
						
												}

												// Praise be to whoever invented copy and paste...
												if (t == "player2_Human") {
														// set to "Computer" player.
														gInfo.SetPlayer (GameInfo._PLAYER2, beings.AI, p2.PlayerName, p2.PlayingAs);
												}
					
												if (t == "player2_O") {
														// set to "X" piece. This means toggling Player 1's piece too.
														gInfo.SetPlayer (GameInfo._PLAYER2, p2.PlayerIs, p2.PlayerName, pieces.X);
														gInfo.SetPlayer (GameInfo._PLAYER1, p1.PlayerIs, p1.PlayerName, pieces.O);
						
												}
					
												if (t == "player2_X") {
														// set to "O" piece...
														gInfo.SetPlayer (GameInfo._PLAYER2, p2.PlayerIs, p2.PlayerName, pieces.O);
														gInfo.SetPlayer (GameInfo._PLAYER1, p1.PlayerIs, p1.PlayerName, pieces.X);
						
												}

												// We're not yet playing a game, so we'll check for the "Play" button here...

												if (t == "btn_PLAY") {

														// Now we need to hide most of the GUI.
														// We only want the "Player [n]: [piece]" part, alternating each turn.
														// So first, we'll hide the rest...

														Player p = gInfo.GetPlayer (GameInfo._PLAYER1);
						
														player1_Computer.gameObject.SetActive (false);
														player1_Human.gameObject.SetActive (false);
														player1_O.gameObject.SetActive (p.PlayingAs == pieces.O);
														player1_X.gameObject.SetActive (p.PlayingAs == pieces.X);

														// Player 1 always plays first, so hide Player 2's info...

														player2_Computer.gameObject.SetActive (false);
														player2_Human.gameObject.SetActive (false);
														player2_O.gameObject.SetActive (false);
														player2_X.gameObject.SetActive (false);

														// PLAY button clicked, so set Game In Progress status:
														board.ClearBoard ();
														gInfo.StartPlaying ();
												
														CheckAndPlayAI ();		// check if current player is an AI, and play his move if so.

						
												}


										}

								}
			
			
						}
				}

		}
}
