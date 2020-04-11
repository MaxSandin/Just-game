﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public Animator animator;
	public Joystick joystick;

	float rotationSpeed = 30;
	Vector3 inputVec;
	Vector3 targetDirection;

	readonly int m_HashSpeed = Animator.StringToHash("Speed");
	readonly int m_HashMoving = Animator.StringToHash("Moving");
	readonly int m_HashHandAttack = Animator.StringToHash("Attack1Trigger");
	readonly int m_HashFootAttack = Animator.StringToHash("Attack2Trigger");
	readonly int m_HashRoolForward = Animator.StringToHash("RollForwardTrigger");
	readonly int m_HashRoolBackward = Animator.StringToHash("RollBackwardTrigger");

	public float attackDuration = 2f;

	// Перекаты
	public enum RoolDirection { Forward, Backward}
	public float rollDuration = 0.35f;
	private bool isRolling = false;
	void Awake()
	{
		SwipeDetector.OnSwipe += SwipeDetector_DoAction;
		DoubleTouchDetector.OnDoubleTouch += DoubleTouchDetector_DoAction;
	}

	void Update()
	{
		//Get input from controls
		float z = joystick.Horizontal;
		float x = joystick.Vertical;
		inputVec = new Vector3(x, 0, z);

		float speed = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));
		animator.SetFloat(m_HashSpeed, speed);

		if (Mathf.Abs(x) >= 0.1 || Mathf.Abs(z) >= 0.1)  //if there is some input
		{
			//set that character is moving
			animator.SetBool(m_HashMoving, true);
		}
		else
		{
			//character is not moving
			animator.SetBool(m_HashMoving, false);
		}

		//update character position and facing
		UpdateMovement();
	}

	//converts control input vectors into camera facing vectors
	void GetCameraRelativeMovement()
	{
		Transform cameraTransform = Camera.main.transform;

		// Forward vector relative to the camera along the x-z plane   
		Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
		forward.y = 0;
		forward = forward.normalized;

		// Right vector relative to the camera
		// Always orthogonal to the forward vector
		Vector3 right = new Vector3(forward.z, 0, -forward.x);

		//directional inputs
		float v = joystick.Vertical;
		float h = joystick.Horizontal;

		// Target direction relative to the camera
		targetDirection = h * right + v * forward;
	}

	//face character along input direction
	void RotateTowardMovementDirection()
	{
		if (inputVec != Vector3.zero)
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDirection), Time.deltaTime * rotationSpeed);
		}
	}

	void UpdateMovement()
	{
		RotateTowardMovementDirection();
		GetCameraRelativeMovement();
	}

	public IEnumerator COStunPause(float pauseTime)
	{
		yield return new WaitForSeconds(pauseTime);
	}

	private void SwipeDetector_DoAction(SwipeDetector.SwipeDirection direction)
	{
		switch (direction)
		{
			case SwipeDetector.SwipeDirection.Up:
				{
					if (!isRolling)
						StartCoroutine(Roll(RoolDirection.Forward));
				}
				break;
			case SwipeDetector.SwipeDirection.Down:
				{
					if (!isRolling)
						StartCoroutine(Roll(RoolDirection.Backward));
				}
				break;
			case SwipeDetector.SwipeDirection.Left:
				{
					animator.SetTrigger(m_HashFootAttack);
					StartCoroutine(COStunPause(attackDuration));
				}
				break;
			case SwipeDetector.SwipeDirection.Right:
				{
					animator.SetTrigger(m_HashHandAttack);
					StartCoroutine(COStunPause(attackDuration));
				}
				break;
		}
	}

	private void DoubleTouchDetector_DoAction(Quaternion rotation)
	{
		if(!isRolling)
		{
			transform.rotation = rotation;
			StartCoroutine(Roll(RoolDirection.Forward));
		}
	}

	public IEnumerator Roll(RoolDirection direction)
	{
		switch(direction)
		{
			case RoolDirection.Forward:
				animator.SetTrigger(m_HashRoolForward);
				break;
			case RoolDirection.Backward:
				animator.SetTrigger(m_HashRoolBackward);
				break;
		}

		isRolling = true;
		yield return new WaitForSeconds(rollDuration);
		isRolling = false;
	}

	//Placeholder functions for Animation events
	void Hit()
	{
	}

	void FootR()
	{
	}

	void FootL()
	{
	}

	void OnGUI()
	{
	}
}