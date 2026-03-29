using UnityEngine;

public static class CommandFactory
{
	public static ICommand Create(NetworkData data, PlayerController player, TowerCamera camera)
	{
		if (data == null)
			return null;

		string type = data.type?.ToUpperInvariant();
		if (type == "ADD_SCORE")
		{
			int amount = 100;
			if (data.payload != null && data.payload.amount > 0)
				amount = (int)data.payload.amount;
			return new AddScoreCommand(amount);
		}

		if (type == "TREE1_COMMAND")
		{
			Debug.Log("Reached CommandFactory TREE1");
			bool flag = true;
			if (data.payload != null && data.payload.flag == true)
				flag = (bool)data.payload.flag;
			return new Tree1Command(flag);

		}

		if (type == "TREE2_COMMAND")
		{
			Debug.Log("Reached CommandFactory TREE2");
			bool flag = true;
			if (data.payload != null && data.payload.flag == true)
				flag = (bool)data.payload.flag;
			return new Tree2Command(flag);

		}

		if (player == null)
			return null;

		switch (type)
		{
			case "MOVE_LEFT":   return new MoveLeftCommand(player);
			case "MOVE_RIGHT":  return new MoveRightCommand(player);
			case "STOP":        return new StopCommand(player);
			case "JUMP":        return new JumpCommand(player);
			case "SPAWN_ITEM":
				string prefabType = data.payload?.prefabType;
				float x = data.payload?.x ?? 0f;
				float y = data.payload?.y ?? 0f;
				return string.IsNullOrEmpty(prefabType) ? null : new SpawnItemCommand(player, prefabType, x, y);

			default:
				return null;
		}
	}
}
