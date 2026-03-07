using UnityEngine;
using System;

public class CommandExecutor : MonoBehaviour
{
	[SerializeField] private PlayerController _player;
	[SerializeField] private NetworkProvider _network;

	public void Execute(NetworkData data)
	{
		if (data == null)
		{
			Debug.LogError("[Executor] Data is null");
			return;
		}

		if (string.IsNullOrEmpty(data.type))
		{
			Debug.LogWarning("[Executor] Message type is empty");
			return;
		}

		switch (data.type.ToUpperInvariant())
		{
			case "HANDSHAKE":
				HandleHandshake(data);
				break;

			case "MOVE_LEFT":
			case "MOVE_RIGHT":
			case "STOP":
			case "JUMP":
			case "SPAWN_ITEM":
			case "ADD_SCORE":
				ExecuteGameCommand(data);
				break;

			default:
				Debug.LogWarning("[Executor] Unknown command type: " + data.type);
				break;
		}
	}

	private void HandleHandshake(NetworkData data)
	{
		SendToServerAsync("{\"type\":\"HANDSHAKE_ACK\",\"client\":\"unity\"}");
	}

	private void ExecuteGameCommand(NetworkData data)
	{
		GameManager gm = GameManager.Instance ?? UnityEngine.Object.FindObjectOfType<GameManager>();
		if (gm != null && gm.GameWon)
		{
			SendToServerAsync("{\"type\":\"GAME_OVER\",\"payload\":{\"message\":\"Game ended.\"}}");
			return;
		}

		bool needPlayer = data.type != null && data.type.ToUpperInvariant() != "ADD_SCORE";
		if (needPlayer && _player == null)
		{
			Debug.LogError("[Executor] PlayerController is not assigned");
			return;
		}

		ICommand command = CommandFactory.Create(data, _player);
		if (command == null)
		{
			Debug.LogWarning("[Executor] Could not create command for: " + data.type);
			return;
		}
		command.Execute();
		SendToServerAsync("{\"type\":\"COMMAND_DONE\",\"payload\":{\"command\":\"" + data.type + "\"}}");
	}

	private async void SendToServerAsync(string json)
	{
		if (_network == null) return;
		await _network.SendAsync(json);
	}
}
