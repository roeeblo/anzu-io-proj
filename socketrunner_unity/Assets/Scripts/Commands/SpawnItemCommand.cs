using UnityEngine;

public class SpawnItemCommand : ICommand
{
	private readonly PlayerController _player;
	private readonly string _prefabType;
	private readonly float _x;
	private readonly float _y;

	public SpawnItemCommand(PlayerController player, string prefabType, float x, float y)
	{
		_player = player;
		_prefabType = prefabType;
		_x = x;
		_y = y;
	}

	public void Execute()
	{
		_player?.SpawnItem(_prefabType, _x, _y);
	}
}
