using UnityEngine;
using System;

[Serializable]
public class NetworkData
{
	public string type;
	public PayloadData payload;
}

[Serializable]
public class PayloadData
{
	public string message;
	public string prefabType;
	public float x;
	public float y;
	public float amount;
}

public class CommandParser : MonoBehaviour
{
	[SerializeField] private CommandExecutor executor;

	public void ParseAndExecute(string json)
	{
		if (string.IsNullOrEmpty(json))
		{
			Debug.LogWarning("[Parser] Received empty json");
			return;
		}

		try
		{
			NetworkData data = UnityEngine.JsonUtility.FromJson<NetworkData>(json);

			if (data == null)
			{
				Debug.LogError("[Parser] Failed to parse json into NetworkData");
				return;
			}

			if (executor == null)
				executor = UnityEngine.Object.FindObjectOfType<CommandExecutor>();
			if (executor == null)
			{
				Debug.LogError("[Parser] Executor is not assigned in Inspector and none found in scene");
				return;
			}

			Debug.Log("[Parser] Parsed message type: " + data.type);
			executor.Execute(data);
		}
		catch (Exception e)
		{
			Debug.LogError("[Parser] Error: " + e.Message);
		}
	}
}