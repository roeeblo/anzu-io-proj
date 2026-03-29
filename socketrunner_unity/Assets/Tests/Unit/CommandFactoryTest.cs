using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class CommandFactoryTest
{
	[Test]
	public void Create_NullData_ReturnsNull()
	{
		Assert.IsNull(CommandFactory.Create(null, null, null));
	}

	[Test]
	public void Create_ADD_SCORE_WithPayloadAmount_ReturnsAddScoreCommand()
	{
		var data = new NetworkData { type = "ADD_SCORE", payload = new PayloadData { amount = 5 } };
		ICommand cmd = CommandFactory.Create(data, null, null);
		Assert.IsNotNull(cmd);
		Assert.IsInstanceOf<AddScoreCommand>(cmd);
	}

	[Test]
	public void Create_ADD_SCORE_NoPayload_ReturnsAddScoreCommand()
	{
		var data = new NetworkData { type = "ADD_SCORE" };
		ICommand cmd = CommandFactory.Create(data, null, null);
		Assert.IsNotNull(cmd);
		Assert.IsInstanceOf<AddScoreCommand>(cmd);
	}

	[Test]
	public void Create_UnknownType_WithNullPlayer_ReturnsNull()
	{
		var data = new NetworkData { type = "MOVE_LEFT" };
		Assert.IsNull(CommandFactory.Create(data, null, null));
	}

	[UnityTest]
	public IEnumerator Create_MOVE_LEFT_WithPlayer_ReturnsMoveLeftCommand()
	{
		var go = new GameObject();
		go.AddComponent<Rigidbody2D>();
		go.AddComponent<BoxCollider2D>();
		var player = go.AddComponent<PlayerController>();
		yield return null;

		var data = new NetworkData { type = "MOVE_LEFT" };
		ICommand cmd = CommandFactory.Create(data, player, null);
		Assert.IsNotNull(cmd);
		Assert.IsInstanceOf<MoveLeftCommand>(cmd);

		Object.DestroyImmediate(go);
	}

	[UnityTest]
	public IEnumerator Create_MOVE_RIGHT_WithPlayer_ReturnsMoveRightCommand()
	{
		var go = new GameObject();
		go.AddComponent<Rigidbody2D>();
		go.AddComponent<BoxCollider2D>();
		var player = go.AddComponent<PlayerController>();
		yield return null;

		var data = new NetworkData { type = "MOVE_RIGHT" };
		ICommand cmd = CommandFactory.Create(data, player, null);
		Assert.IsNotNull(cmd);
		Assert.IsInstanceOf<MoveRightCommand>(cmd);

		Object.DestroyImmediate(go);
	}

	[UnityTest]
	public IEnumerator Create_STOP_WithPlayer_ReturnsStopCommand()
	{
		var go = new GameObject();
		go.AddComponent<Rigidbody2D>();
		go.AddComponent<BoxCollider2D>();
		var player = go.AddComponent<PlayerController>();
		yield return null;

		var data = new NetworkData { type = "STOP" };
		ICommand cmd = CommandFactory.Create(data, player, null);
		Assert.IsNotNull(cmd);
		Assert.IsInstanceOf<StopCommand>(cmd);

		Object.DestroyImmediate(go);
	}

	[UnityTest]
	public IEnumerator Create_JUMP_WithPlayer_ReturnsJumpCommand()
	{
		var go = new GameObject();
		go.AddComponent<Rigidbody2D>();
		go.AddComponent<BoxCollider2D>();
		var player = go.AddComponent<PlayerController>();
		yield return null;

		var data = new NetworkData { type = "JUMP" };
		ICommand cmd = CommandFactory.Create(data, player, null);
		Assert.IsNotNull(cmd);
		Assert.IsInstanceOf<JumpCommand>(cmd);

		Object.DestroyImmediate(go);
	}

	[UnityTest]
	public IEnumerator Create_SPAWN_ITEM_WithPayload_ReturnsSpawnItemCommand()
	{
		var go = new GameObject();
		go.AddComponent<Rigidbody2D>();
		go.AddComponent<BoxCollider2D>();
		var player = go.AddComponent<PlayerController>();
		yield return null;

		var data = new NetworkData
		{
			type = "SPAWN_ITEM",
			payload = new PayloadData { prefabType = "gem", x = 1f, y = 2f }
		};
		ICommand cmd = CommandFactory.Create(data, player, null);
		Assert.IsNotNull(cmd);
		Assert.IsInstanceOf<SpawnItemCommand>(cmd);

		Object.DestroyImmediate(go);
	}

	[UnityTest]
	public IEnumerator Create_SPAWN_ITEM_EmptyPrefabType_ReturnsNull()
	{
		var go = new GameObject();
		go.AddComponent<Rigidbody2D>();
		go.AddComponent<BoxCollider2D>();
		var player = go.AddComponent<PlayerController>();
		yield return null;

		var data = new NetworkData { type = "SPAWN_ITEM", payload = new PayloadData { prefabType = "" } };
		Assert.IsNull(CommandFactory.Create(data, player, null));

		Object.DestroyImmediate(go);
	}
}
