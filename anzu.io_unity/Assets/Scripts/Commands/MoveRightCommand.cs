using UnityEngine;

public class MoveRightCommand : ICommand
{
	private readonly PlayerController _player;

	public MoveRightCommand(PlayerController player)
	{
		_player = player;
	}

	public void Execute()
	{
		_player?.MoveRight();
	}
}
