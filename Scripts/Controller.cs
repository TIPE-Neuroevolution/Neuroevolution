﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Controller : MonoBehaviour {

	#region variables
	public List<Node> nodes;
	public List<Muscle> muscles;

	public Text cycleText;
	public Text distanceText;
	public Text timeText;

	public float cycleDuration;

	private float currentTime = 0;
	#endregion


	#region Update
	void Update () {
		/*
		 * time modulo cycle duration
		 */
		var time = (currentTime - cycleDuration * (Mathf.FloorToInt (currentTime / cycleDuration)));

		/*
		 * Update muscles and nodes
		 */
		foreach(var m in muscles) {
			m.Update (time);
		}
		foreach(var n in nodes) {
			n.Update (Time.deltaTime * Constants.timeMultiplier);
		}
		foreach(var m in muscles) {
			m.LateUpdate ();
		}
		foreach(var n in nodes) {
			n.LateUpdate ();
		}

		/*
		 * Update average position
		 */
		float avPosition = 0;
		foreach(var n in nodes) {
			avPosition += n.position.x;
		}
		avPosition /= nodes.Count;

		var tmp = transform.position;
		tmp.x = avPosition;
		transform.position = tmp;

		/*
		 * Update UI
		 */
		distanceText.text = "Distance : " + avPosition.ToString ();
		timeText.text = "Time : " + currentTime.ToString ();
		cycleText.text = (Mathf.Ceil (time / cycleDuration * 100)).ToString () + " %";

		/*
		 * Update current time
		 */
		currentTime += Time.deltaTime * Constants.timeMultiplier;
	}
	#endregion
}