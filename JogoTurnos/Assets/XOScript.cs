using UnityEngine;
using System.Collections;

public class XOScript : MonoBehaviour
{

		private bool _isX;

		// Use this for initialization
		void Start ()
		{

				if (gameObject.CompareTag ("cellX")) {
						_isX = true;
				} else {
						_isX = false;
				}
		}

		// The following functions handle showing / hiding the symbol this script is attached to...
	
		public void HideX ()
		{
				if (_isX) {
						gameObject.GetComponent<Renderer>().enabled = false;
				}
		}

		public void ShowX ()
		{
				if (_isX) {
						gameObject.GetComponent<Renderer>().enabled = true;
				}
		}

		public void HideO ()
		{
				if (!_isX) {
						gameObject.GetComponent<Renderer>().enabled = false;
				}
		}

		public void ShowO ()
		{
				if (!_isX) {
						gameObject.GetComponent<Renderer>().enabled = true;
				}
		}


}
