using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{

	private Player _player;
	private float _lStickHorizontal;
	private float _lStickVertical;
	public Direction direction;
	public Direction storedDirection;
	
	public float _sensitivity;	//from 0 to 1, 0 is the most sensitive. Now put on 0.95
	private Direction newDirection;

	public void CustomStart(Direction direction)
	{
		_player = GetComponent<Player>();
		this.direction = direction;
		storedDirection = direction;
	}

	public void UpdateMovementInput(int playerId, bool storeDirection)
	{
		newDirection = Direction.None;
		if (!_player._haveToPlayWithKeyboard)
		{
			_lStickHorizontal = Input.GetAxis("HorizontalAxis" + playerId) * _player._directionController;
			_lStickVertical = Input.GetAxis("VerticalAxis" + playerId) * _player._directionController;

			newDirection = Direction.None;

			if (_lStickHorizontal > _sensitivity)
				newDirection = Direction.Right; //right
			if (_lStickHorizontal < -_sensitivity)
				newDirection = Direction.Left; //left
			if (_lStickVertical > _sensitivity)
				newDirection = Direction.Down; //up
			if (_lStickVertical < -_sensitivity)
				newDirection = Direction.Up; //down
		}
		else
		{
			if (_player._directionController == 1)
				NormalInputKeyboard(playerId);
			else
				InvertInputKeyboard(playerId);
		}

		if (storeDirection) //MOVING
		{
			if (newDirection != Direction.None)
			{
				storedDirection = newDirection;
			}
		}
		else //IDLE
		{
			if (storedDirection != direction)
			{
				direction = storedDirection;
				FaceToDirection();
			}
			if (newDirection != Direction.None)
			{
				direction = newDirection;
				storedDirection = direction;
				FaceToDirection();
			}
		}
	}

	private void NormalInputKeyboard(int playerId)
	{
		switch (playerId)
		{
			case 0:
				if (Input.GetKey(KeyCode.A))
					newDirection = Direction.Left;
				else if (Input.GetKey(KeyCode.D))
					newDirection = Direction.Right;
				else if (Input.GetKey(KeyCode.W))
					newDirection = Direction.Up;
				else if (Input.GetKey(KeyCode.S))
					newDirection = Direction.Down;
				break;
			case 1:
				if (Input.GetKey(KeyCode.LeftArrow))
					newDirection = Direction.Left;
				else if (Input.GetKey(KeyCode.RightArrow))
					newDirection = Direction.Right;
				else if (Input.GetKey(KeyCode.UpArrow))
					newDirection = Direction.Up;
				else if (Input.GetKey(KeyCode.DownArrow))
					newDirection = Direction.Down;
				break;
			case 2:
				if (Input.GetKey(KeyCode.Keypad4))
					newDirection = Direction.Left;
				else if (Input.GetKey(KeyCode.Keypad6))
					newDirection = Direction.Right;
				else if (Input.GetKey(KeyCode.Keypad8))
					newDirection = Direction.Up;
				else if (Input.GetKey(KeyCode.Keypad5) || Input.GetKey(KeyCode.Keypad2))
					newDirection = Direction.Down;
				break;
			case 3:
				if (Input.GetKey(KeyCode.J))
					newDirection = Direction.Left;
				else if (Input.GetKey(KeyCode.L))
					newDirection = Direction.Right;
				else if (Input.GetKey(KeyCode.I))
					newDirection = Direction.Up;
				else if (Input.GetKey(KeyCode.K))
					newDirection = Direction.Down;
				break;
		}
	}
	
	private void InvertInputKeyboard(int playerId)
	{
		switch (playerId)
		{
			case 0:
				if (Input.GetKey(KeyCode.A))
					newDirection = Direction.Right;
				else if (Input.GetKey(KeyCode.D))
					newDirection = Direction.Left;
				else if (Input.GetKey(KeyCode.W))
					newDirection = Direction.Down;
				else if (Input.GetKey(KeyCode.S))
					newDirection = Direction.Up;
				break;
			case 1:
				if (Input.GetKey(KeyCode.LeftArrow))
					newDirection = Direction.Right;
				else if (Input.GetKey(KeyCode.RightArrow))
					newDirection = Direction.Left;
				else if (Input.GetKey(KeyCode.UpArrow))
					newDirection = Direction.Down;
				else if (Input.GetKey(KeyCode.DownArrow))
					newDirection = Direction.Up;
				break;
			case 2:
				if (Input.GetKey(KeyCode.Keypad4))
					newDirection = Direction.Right;
				else if (Input.GetKey(KeyCode.Keypad6))
					newDirection = Direction.Left;
				else if (Input.GetKey(KeyCode.Keypad8))
					newDirection = Direction.Down;
				else if (Input.GetKey(KeyCode.Keypad5) || Input.GetKey(KeyCode.Keypad2))
					newDirection = Direction.Up;
				break;
			case 3:
				if (Input.GetKey(KeyCode.J))
					newDirection = Direction.Right;
				else if (Input.GetKey(KeyCode.L))
					newDirection = Direction.Left;
				else if (Input.GetKey(KeyCode.I))
					newDirection = Direction.Down;
				else if (Input.GetKey(KeyCode.K))
					newDirection = Direction.Up;
				break;
		}
	}
	
	public void FaceToDirection()
	{
		switch (direction)
		{
			case Direction.Up:
				transform.rotation = Quaternion.LookRotation(Vector3.forward);
				break;
			case Direction.Down:
				transform.rotation = Quaternion.LookRotation(Vector3.back);
				break;
			case Direction.Left:
				transform.rotation = Quaternion.LookRotation(Vector3.left);
				break;
			case Direction.Right:
				transform.rotation = Quaternion.LookRotation(Vector3.right);
				break;
			case Direction.None:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
	
	public void UpdateAbilityInputs(int playerId)
	{		
		if (!_player.CanUseAbilitiesThisTurn() || _player._currentAbilityUsable == null)
			return;

		
		if (!_player._haveToPlayWithKeyboard)
		{
			if (Input.GetButtonDown("AbilityButton" + playerId))
				_player.InputAbility();
		}
		else
		{
			switch (playerId)
			{
				case 0:
					if (Input.GetKey(KeyCode.LeftShift))
						_player.InputAbility();
					break;
				case 1:
					if (Input.GetKey(KeyCode.RightShift))
						_player.InputAbility();
					break;
				case 2:
					if (Input.GetKey(KeyCode.Keypad0))
						_player.InputAbility();
					break;
				case 3:
					if (Input.GetKey(KeyCode.B))
						_player.InputAbility();
					break;
			}
		}
	}
}