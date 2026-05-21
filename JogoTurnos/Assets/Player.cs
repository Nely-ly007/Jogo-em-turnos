using UnityEngine;
using System.Collections;

/* 
 * Defines a Player in the game.
 * 
 * A Player can be either a Human or a Computer.
 * A Player can be either 'X' or 'O' pieces.
 * A Player can be 'Playing', or 'Waiting'. ('Idle' usually means 'awaiting turn'.)
 * A Player has a score. (Not used in the tutorial version.)
 * A Player has a win-loss count.
 * 
 * A Player currently cannot have a name. This is left as an exercise for the student.

 */

// Define some useful enumerations to help readability. 
// These are placed outside the class definition so we don't need to prepend the class name whenever we use these.

public enum pieces
{
		X,
		O }
;
public enum beings
{
		Human,
		AI }
;



public class Player
{


		private bool _isX = false;	// I prefer to prepend an underscore to denote a private member variable with accessors.
		public pieces PlayingAs {
				get {
						return _isX ? pieces.X : pieces.O;
				}
				set {
						if (value == pieces.X)
								_isX = true;
						else if (value == pieces.O)
								_isX = false;
						// any other values are ignored.
				}
		}

		// PlayerName will be used in the next release. It is not used in Tic-Tac-Two.
		private string _name = string.Empty;
		public string PlayerName {
				get {
						return _name;
				}
				set {
						_name = value;
				}
		}

		private bool _isHuman = false;
		public beings PlayerIs {
				get {
						return _isHuman ? beings.Human : beings.AI;
				}
				set {
						if (value == beings.Human)
								_isHuman = true;
						else if (value == beings.AI)
								_isHuman = false;
						// any other values are ignored.

				}
		}

		// This scoring / stats data will be used in the next version, once the new Unity GUI system is ready.
	
		public int score = 0;

		public int won = 0;
		public int lost = 0;
		public int drawn = 0;		// in theory, we could calculate one of these three values using 'totalGames' to provide the necessary info, but code is harder to maintain than data.
		public int totalGames = 0;


}
