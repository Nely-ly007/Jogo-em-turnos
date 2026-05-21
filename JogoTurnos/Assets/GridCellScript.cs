using UnityEngine;
using System.Collections;

/*
 * - GridCellScript - 
 * 
 * If you've seen the previous version of Tic-Tac-Tut, this fulfils much the same role as that version's cell script.
 * 
 * The idea is for each cell to maintain its own "empty", "x" and "o" state.
 * 
 */

public class GridCellScript : MonoBehaviour
{

		
		public enum CellStates
		{
				empty,
				O,
				X}
		;

		private CellStates _state;

		private XOScript[] xoScripts;


		private bool stateHasChanged = false;

		public CellStates state {
				get {
						return _state;
				}
				set {
						_state = value;
						stateHasChanged = true;
						if (value != CellStates.empty) {		// We'll only set the cell to empty if we're wiping the board, so don't refresh the scores yet.
								SendMessageUpwards ("RefreshScores");	// notify BoardManager that it needs to update its score arrays.
						}
				}

		}
		
		// Use this for initialization
		void Start ()
		{

				xoScripts = gameObject.GetComponentsInChildren<XOScript> ();

				if (xoScripts == null)
						Debug.Log ("XO Scripts Array is EMPTY");
		
				state = CellStates.empty;	// all cells are initially empty.
		}
	
		// Update is called once per frame
		void LateUpdate ()
		{

				if (stateHasChanged) {
						if (state == CellStates.empty) {
								foreach (XOScript xs in xoScripts) {
										xs.HideO ();
										xs.HideX ();
								}
						}
						if (state == CellStates.O) {
								foreach (XOScript xs in xoScripts) {
										xs.ShowO ();
								}
						}
						if (state == CellStates.X) {
								foreach (XOScript xs in xoScripts) {
										xs.ShowX ();
								}
						}
						
				}
				stateHasChanged = false;		// don't want to keep refreshing the board every frame.
		}
}
