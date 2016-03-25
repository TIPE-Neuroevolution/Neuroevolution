﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//TODO: Reset && Disable
public class Node {
	
	#region public variables
	public float friction;
	public float mass;
	public float nodeRadius;
	public float coefficientOfRestitution;
	public int id;

	public Vector2 position;
	#endregion

	#region private and protected variables
	protected NodeRenderer nodeRenderer;
	protected Vector2 forcesSum = Vector2.zero;
	protected Vector2 velocitySum = Vector2.zero;
	protected Vector2 constraintSum = Vector2.zero;
	private Vector2 previousSpeed = Vector2.zero;
	#endregion


	#region Constructor
	public Node () {}
	public Node (float friction, Vector2 position, float mass, float coefficientOfRestitution, Transform parent, int id) {
		this.friction = friction;
		this.position = position;
		this.mass = mass;
		this.coefficientOfRestitution = coefficientOfRestitution;
		this.id = id;

		//Create node renderer
		var go = GameObject.Instantiate (Resources.Load ("Circle"), position, Quaternion.identity) as GameObject;
		go.name = "Node " + id.ToString ();
		go.GetComponent<SpriteRenderer> ().color = new Color (friction / Constants.frictionAmplitude, 0, 0);
		nodeRenderer = go.AddComponent<NodeRenderer> ();
		nodeRenderer.id = id;
		nodeRenderer.transform.parent = parent;

		nodeRadius = go.GetComponent<SpriteRenderer> ().bounds.extents.x;
	}
	#endregion

	#region Update and LateUpdate
	public virtual void Update (float deltaTime) {
		//Weight
//		AddForce (Vector2.down * Constants.gravityMultiplier * mass);

		//Fluid friction
//		AddForce (-previousSpeed * Constants.fluidFriction);

		//v = a * dt + c
        var velocity = forcesSum * deltaTime / mass + previousSpeed + velocitySum;

		velocity -= constraintSum.normalized * Vector2.Dot (velocity, constraintSum.normalized);

		velocity += Vector2.down * Constants.gravityMultiplier * deltaTime;


		if ((velocity * deltaTime + position).y < nodeRadius) {
			if (position.y > nodeRadius + Constants.tolerance)
				velocity.y = (nodeRadius - position.y) / deltaTime;
			else
				velocity.y -= (1 + coefficientOfRestitution) * velocity.y;
		}
//
		if (position.y < nodeRadius + Constants.tolerance)
			velocity.x /= (1 + friction);

//		velocity -= velocitySum.normalized * Vector2.Dot (velocity, velocitySum);

		//delta position = v * dt
		var deltaPosition = velocity * deltaTime;

		position += deltaPosition;
			
		forcesSum = Vector2.zero;
		velocitySum = Vector2.zero;
		constraintSum = Vector2.zero;

		previousSpeed = velocity;
	}

	public void LateUpdate () {
		nodeRenderer.SetPosition (position);
	}
	#endregion

	#region AddForce
	public void AddForce (Vector2 force) {
		forcesSum += force;
	}

	public void AddVelocity (Vector2 velocity) {
		velocitySum += velocity;
	}

	public void AddConstraint(Vector2 constraint) {
		constraintSum += constraint;
	}
	#endregion


	#region Destroy
	public void Destroy () {
		MonoBehaviour.Destroy (nodeRenderer.gameObject);
	}
	#endregion

	public static Node RandomNode (Vector2 position, Transform parent, int id) {
		var friction = Random.Range (Constants.minRandom, Constants.frictionAmplitude);
		var mass = Random.Range (Constants.minMass, Constants.maxMass);

		return new Node (friction, position, mass, Constants.bounciness, parent, id);
	}
}