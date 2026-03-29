using UnityEngine;

public class JumpCommand : ICommand
{
	private readonly PlayerController _player;

	public JumpCommand(PlayerController player)
	{
		_player = player;
	}

	public void Execute()
	{
		_player?.Jump();
	}
}
