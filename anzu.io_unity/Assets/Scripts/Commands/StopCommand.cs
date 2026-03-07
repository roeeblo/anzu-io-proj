using UnityEngine;

public class StopCommand : ICommand
{
	private readonly PlayerController _player;

	public StopCommand(PlayerController player)
	{
		_player = player;
	}

	public void Execute()
	{
		_player?.Stop();
	}
}
