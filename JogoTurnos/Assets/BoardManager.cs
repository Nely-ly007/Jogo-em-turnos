using UnityEngine;
using System.Collections;

// This class provides board-related convenience functions.

public class BoardManager : MonoBehaviour
{

		public const int BOARDWIDTH = 3;
		public const int BOARDHEIGHT = 3;
		public const int BOARDDIMENSIONS = BOARDWIDTH * BOARDHEIGHT;		// maximum no. cells in the board.
		
		public const float RANDOMFACTOR = 0.5f;		// Used in the cell scoring code to ensure we don't just pick the first cell if multiple identically-scored 'best move' cells are found.

		public const int EMPTYCELL = 0;
		public const int O_CELL = 1;
		public const int X_CELL = 2;
	
		// Holds references to our cells...
		public GridCellScript[] gridCells;

		// the following will make some of the AI scoring functions easier to read...
		// REMINDER: These constants are also used by "GUIHandler.cs" to convert between the two piece datatypes.
		


		private GUIHandler gHandler;
		
		// An enum containing compass directions. Used by the AI scoring functions.
		// (I've prefixed each direction with a "D_" mainly to stop MonoDevelop's autocomplete feature from making silly suggestions.)

		private enum Directions
		{
				D_N,
				D_NE,
				D_E,
				D_SE,
				D_S,
				D_SW,
				D_W,
				D_NW }
		;

		private int pieceToPlayNext;	// tells who's playing next, X or O.

		// We need somewhere to maintain the AI scoring for each cell.
		// When the score is requested, the BoardManager only needs to know which token needs to play, not which player is using that token.

		private int[] scoreBoard;	// holds the scores for the AI.


		// Use this for initialization
		void Start ()
		{
				gHandler = FindObjectOfType<GUIHandler> ();

				if (gHandler == null)
						Debug.Log ("ERROR: Could not find GUIHandler script!");

				scoreBoard = new int[BOARDDIMENSIONS];

				InitScoreboard ();
				
		}

		// Some utility functions...

		void InitScoreboard ()
		{
				// Initialise the score tables.
				pieceToPlayNext = EMPTYCELL;		// we don't know which piece is playing first at this stage.
		
				// setup initial scores...
				// We'll toss a virtual coin to decide whether to score the centre square highly.
		
				for (int i = 0; i < scoreBoard.Length; ++i) {
						scoreBoard [i] = 0;
			
						if (i == 4) {
								scoreBoard [i] = (Random.value >= RANDOMFACTOR) ? 5 : 0;
						}
				}
		}
	
		public void ClearBoard ()
		{
				foreach (GridCellScript gcs in gridCells) {
						gcs.state = GridCellScript.CellStates.empty;
				}

				// reset the scoreboard:
				InitScoreboard ();

		}

		private int [] GetBoardAsInts ()
		{

				// The next two counters are used to keep track of how many of each piece are on the board.
				// We use this to work out which piece is playing next, so we know how to calculate the AI scoreboard.
				int xCount = 0;
				int oCount = 0;

				int [] cells;
				int i = 0;

				cells = new int[BOARDDIMENSIONS];

				foreach (GridCellScript gcs in gridCells) {
						if (gcs.state == GridCellScript.CellStates.empty) {
								cells [i] = EMPTYCELL;
						}
						if (gcs.state == GridCellScript.CellStates.O) {
								cells [i] = O_CELL;
								++oCount;
						} 
						if (gcs.state == GridCellScript.CellStates.X) {
								cells [i] = X_CELL;
								++xCount;
						}

						++i;

				}

				// Work out which piece we need to recalculate board for: X or O...

				// Let's ask the GUIHandler script for the current player.
				// BUT!!!
				// We actually prepare the scores for the next player BEFORE
				// the GUIHandler calls "NextTurn()"!
				// So we swap the result -- if it's 'X', the next piece will be 'O', and vice-versa...
				
				pieceToPlayNext = (gHandler.GetCurrentPlayerPieceAsInt ()) == X_CELL ? O_CELL : X_CELL;

				return cells;
		}

		// Called by AIPlayer script to play its move.
		public void AIPlayMoveAt (int cell, pieces piece)
		{
			
				GridCellScript gcs = gridCells [cell];
				gcs.state = (piece == pieces.X) ? GridCellScript.CellStates.X : GridCellScript.CellStates.O;
				
				gHandler.NextTurn ();	// trigger next turn.
		}

		// Check if game has been won by either side.
		// This function is a lot simpler than it looks. I've added reams of comments to explain what's going on (and also to help me write the docs afterwards), but
		//  the actual _code_ is very short and sweet.

		private int HasGameBeenWon (ref int[] board)
		{
				int winner = EMPTYCELL;

				// Set up the eight win states:

				// In order these are: top row, middle row, bottom row, left column, middle column, right column, diagonal from top-left ("\"), diagonal from top-right ("/").
				uint [] winStates = { 448u, 56u, 7u, 292u, 146u, 73u, 273u, 84u };
				// A "uint" is an "unsigned" integer: a whole number that is always positive. In contrast, a normal int can be positive or negative (i.e. "signed").
				// (The trailing 'u' after each number in the array above just means "unsigned integer". It's not strictly necessary, but it's become a habit and it does no harm.)

				// The reason for choosing an unsigned integer here is to eliminate the possibility of subtle bugs due to unexpected negative numbers.
				// In theory, it's not strictly necessary, but if it's possible to eliminate an entire class of potential bugs through such a simple modification, it makes sense to do so.

				// First, grab the cell states for 'O':

				uint OCells = 0;	// we're going to use some basic binary arithmetic for this test:
				uint XCells = 0;

				int i = 8;	// this is used as a _bit_ counter, not an int counter.

				// The idea is to set a bit to 1 (i.e. 'true') if there's a corresponding piece in that position.
				// The result should be a single unsigned integer containing a single bit for each cell on the board.
				// If that cell contains an 'O', the corresponding bit is set to 1, otherwise it's left at 0.
				// To do this, we use 'i' to tell us how many bits the '1' needs to be shifted along by, then use
				// the "logical OR" ( the single vertical bar, or 'solidus', symbol before the equals) operation
				// to insert that bit into our OCells variable.

				// We need to use the "logical OR" operator otherwise we'd obliterate any other '1' bits in the O/XCells
				// variable whenever we assign the value.

				foreach (int cell in board) {
						// update OCells and XCells according to the board cell states...
						if (cell == O_CELL)
								OCells |= (1u << i);
						if (cell == X_CELL)
								XCells |= (1u << i);

						if (i > 0)	// A quick sanity-check.
								--i;	// The top-left corner of the board should be the left-most bit, so we're storing from bit 8 to bit 0.
				}

				// Now check if we have one of the eight 'win' conditions. Because we've got all our cells stored as
				// binary digits ('bits') in a single variable, we can do this with just nine simple tests.

				// First, we'll test for horizontal rows. I.e. "111000000", "000111000" or "000000111".
				// If you don't have a programmer's calculator installed, it's well worth doing so. I'm using the Apple Calculator app in OS X, which includes
				// a "Programmer's Calculator" mode. (It's also free and comes with OS X.) It lets you convert easily between binary and decimal numbers.
				// Similar calculator apps are also available for Windows and most mobile / smartphone platforms too. I recommend PCalc for iOS devices.


				foreach (uint state in winStates) {
						if ((OCells & state) == state) {
								winner = O_CELL;
						}
						if ((XCells & state) == state) {
								winner = X_CELL;
						}
				}


				// and we're done!

				return (winner);


		}
		
		// This message is sent by a GridCellScript if the cell has been changed.
		// It tells us we need to update the scoring arrays.
	
		public void RefreshScores ()
		{
				int [] board = GetBoardAsInts ();		// This also works out who's turn it is now.
		
				int winningPiece = HasGameBeenWon (ref board);	// returns EMPTYCELL if neither side has won yet.
		
		
		
				// First check if the game has been drawn. If so, set game states accordingly.
		
				int playableCells = 0;
				foreach (int c in board) {
						if (c == EMPTYCELL)
								++playableCells;
				}
				// Note that we need to be careful with this check, because it's possible
				//  for a player to win by playing the last empty cell on the board.
				// Hence the two tests here:
				if ((playableCells <= 0) && (winningPiece == EMPTYCELL)) {	// it's a draw...
						gHandler.GameDrawn ();
				} 
		
				if (winningPiece == EMPTYCELL) {
						// nobody has won, we're still playing, so continue with scoring process...
			
						for (int i = 0; i < BOARDDIMENSIONS; ++i) {
								scoreBoard [i] = GetScoreFor (pieceToPlayNext, i, ref board);
						}
			
				} else {
			
						gHandler.GameWon ();
				}
		
		}
	
		// This function is called by the AI code to find out the best cell to play.
		public int GetBestMove ()
		{
				int bestScore = 0;
				int i = 0, bestCell = -1;
				foreach (int score in scoreBoard) {
						if (score > bestScore) {
								bestScore = score;
								bestCell = i;
						}
						// If we have multiple cells with the same best score, this next bit ensures we don't always pick the first one.
						// It adds a little randomness to the proceedings.
						if (score == bestScore) {
								if (Random.value > RANDOMFACTOR)	// Random.value always returns a random number between 0.0 and 1.0 (inclusive).
										bestCell = i; 
						}
						++i;
				}
		
				return (bestCell);	// note that we're returning the cell array position, not an actual score. The scoring system is completely hidden within this file.
		}
	
	
		
		

	
		// This function sets the score for a specified piece ("O" or "X") at the specified cell.
		// The game board's current state is passed by reference. The score we return will be placed in a scoreboard array.

		private int GetScoreFor (int nextPlayer, int i, ref int[] board)
		{

				if (board [i] != EMPTYCELL) {
						return (-1);		// Cell is already occupied.
				}
				
				if (i == 4 && board [i] == EMPTYCELL && Random.value > RANDOMFACTOR) {	// nobody has played the centre square yet, so nudge current player to play it.
						return (5);
				}

				// If we've got this far, we know the cell is playable.
				// We now check in each direction.

				int friendCellsCount;
				int foeCellsCount;
				
				int score = 0;
				
				// Now, search in each of the eight compass directions to find out what the situation is.
				// Note that we do NOT pass "i" by reference: we want to keep its value intact.
				// Also, note that we're grouping the directions to obtain scores for complete lines / diagonals, not just segments of lines.
				
				friendCellsCount = 0;
				foeCellsCount = 0;
				SetCellsCountersFor (nextPlayer, i, ref friendCellsCount, ref foeCellsCount, ref board, Directions.D_N);
				SetCellsCountersFor (nextPlayer, i, ref friendCellsCount, ref foeCellsCount, ref board, Directions.D_S);
					
				score += CalcScore (friendCellsCount, foeCellsCount);				

				friendCellsCount = 0;
				foeCellsCount = 0;
				SetCellsCountersFor (nextPlayer, i, ref friendCellsCount, ref foeCellsCount, ref board, Directions.D_NE);
				SetCellsCountersFor (nextPlayer, i, ref friendCellsCount, ref foeCellsCount, ref board, Directions.D_SW);

				score += CalcScore (friendCellsCount, foeCellsCount);
		
				friendCellsCount = 0;
				foeCellsCount = 0;
				SetCellsCountersFor (nextPlayer, i, ref friendCellsCount, ref foeCellsCount, ref board, Directions.D_E);
				SetCellsCountersFor (nextPlayer, i, ref friendCellsCount, ref foeCellsCount, ref board, Directions.D_W);
				
				score += CalcScore (friendCellsCount, foeCellsCount);
		
				friendCellsCount = 0;
				foeCellsCount = 0;
				SetCellsCountersFor (nextPlayer, i, ref friendCellsCount, ref foeCellsCount, ref board, Directions.D_SE);
				SetCellsCountersFor (nextPlayer, i, ref friendCellsCount, ref foeCellsCount, ref board, Directions.D_NW);

				score += CalcScore (friendCellsCount, foeCellsCount);
		
				return score;
		
		}

		// Counts the friendly and enemy pieces in the direction specified.
		// This is a "recursive" function: it calls itself. This is explained in more detail in the documentation.

		private void SetCellsCountersFor (int friendPiece, int pos, ref int friendCounter, ref int foeCounter, ref int[] board, Directions dir)
		{
				// Check we're not out of bounds:
				int foePiece = (friendPiece == X_CELL) ? O_CELL : X_CELL;
				
				// In theory, this code shouldn't be necessary, but recursion bugs can be nasty. Hope for the best, but be prepared for the worst...
		
				if (pos < 0 || pos >= BOARDDIMENSIONS) {
						Debug.Log ("ERROR: SetCellsCountersFor() - pos out of bounds!");
						return;
				}
		
		
				// we'll take advantage of an assumption here: we know we're starting this process from an empty cell, so we can do this:
				if (board [pos] == friendPiece) {
						++friendCounter;
				}
				if (board [pos] == foePiece) {
						++foeCounter;
				}


				// now comes the recursive part:

				switch (dir) {
				case Directions.D_N:
				// search up
						if (pos >= BOARDWIDTH) {
								SetCellsCountersFor (friendPiece, (pos - BOARDWIDTH), ref friendCounter, ref foeCounter, ref board, dir);
						}
						break;
			
				case Directions.D_NE:
				// search diagonally (up-right), if possible.
						if (pos == 4 || pos == 6) {	// checking diagonally in the 'NE' direction is pointless from the other positions
								SetCellsCountersFor (friendPiece, (pos - (BOARDWIDTH - 1)), ref friendCounter, ref foeCounter, ref board, dir);
						}
						break;
			
				case Directions.D_E:
				// search right
						if (pos != 2 && pos != 5 && pos != 8) {	// don't want to go beyond the right edge of the board.
								SetCellsCountersFor (friendPiece, (++pos), ref friendCounter, ref foeCounter, ref board, dir);
						}
						break;
			
				case Directions.D_SE:
				// search diagonally (down-right), if possible. (Note that only the diagonals through the centre square count.)
						if (pos == 0 || pos == 4) {
								SetCellsCountersFor (friendPiece, (pos + (BOARDWIDTH + 1)), ref friendCounter, ref foeCounter, ref board, dir);
						}
						break;
				
				case Directions.D_S:
				// search down
						if (pos < 6) {
								SetCellsCountersFor (friendPiece, (pos + BOARDWIDTH), ref friendCounter, ref foeCounter, ref board, dir);
						}
						break;
				
				case Directions.D_SW:
				// search diagonally (down-left), if possible.
						if (pos == 2 || pos == 4) {
								SetCellsCountersFor (friendPiece, (pos + (BOARDWIDTH - 1)), ref friendCounter, ref foeCounter, ref board, dir);
						}
						break;
				
				case Directions.D_W:
				// search left
						if (pos != 0 && pos != 3 && pos != 6) {	// don't want to go off the left edge of the board
								SetCellsCountersFor (friendPiece, (--pos), ref friendCounter, ref foeCounter, ref board, dir);
						}
						break;
				
				case Directions.D_NW:
				// search diagonally (up-left)
						if (pos == 4 || pos == 8) {
								SetCellsCountersFor (friendPiece, (pos - (BOARDWIDTH + 1)), ref friendCounter, ref foeCounter, ref board, dir);
						}
						break;
				}
		
				// When our recursion has run into one of our bounds tests, we fall through to here.
				// The result should be correctly set counters.
				return;
		
		}
		
		// This is the AI scoring calculation itself. Called by GetScoreFor().
		private int CalcScore (int friendCellsCount, int foeCellsCount)
		{
				int temp = 0;
				if (friendCellsCount == 1) {
						temp += 2;		// slightly encourage playing lines, not random squares.
				}
				if (foeCellsCount == 1) {
						temp += 1;
				}
		
				if (friendCellsCount > 1) {	// we can win by playing this square.	
						temp += 20;
				}
				if (foeCellsCount > 1) {	// or we can stop our opponent from winning by playing it.
						temp += 9; 
				}
		
				// Implicit: if the entire line is empty, the score is 0.		
		
				return temp;
		}
		
		// This is called by the receiver of the "GameWon" message, to work out who won.
		public bool WasWinnerX ()
		{
				// The "pieceToPlayNext" variable tells us who was due to play the next turn.
				// But the game has already been won, so we just need to return the opposite piece...

				// Note that BoardManager doesn't know about GameInfo or GUIHandler at all, so we have to pass this back as a simple boolean.
				// The return value answers the question posed by the function's name.

				return ((pieceToPlayNext == O_CELL) ? true : false);	// translation: was "O" due to play the next turn? If so, then "X" won.
		}


}
