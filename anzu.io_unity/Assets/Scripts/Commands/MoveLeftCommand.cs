using UnityEngine;

public class MoveLeftCommand : ICommand
{
	private readonly PlayerController _player;

	public MoveLeftCommand(PlayerController player)
	{
		_player = player;
	}

	public void Execute()
	{
		_player?.MoveLeft();
	}
}
