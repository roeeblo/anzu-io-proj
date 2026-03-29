using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Reflection;

public class PlayerControllerTest
{
	private GameObject _playerGo;
	private PlayerController _player;
	private Rigidbody2D _rb;

	private static readonly MethodInfo FixedUpdateMethod =
		typeof(PlayerController).GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic);

	private static FieldInfo FindField(System.Type t, System.Type fieldType)
	{
		foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
		{
			if (f.FieldType == fieldType)
				return f;
		}
		return null;
	}

	private static readonly FieldInfo PlayerRbField = FindField(typeof(PlayerController), typeof(Rigidbody2D));
	private static readonly FieldInfo PlayerColField = FindField(typeof(PlayerController), typeof(Collider2D));

	private static void WirePlayerPhysics(PlayerController player, Rigidbody2D rb, Collider2D col)
	{
		if (PlayerRbField != null)
			PlayerRbField.SetValue(player, rb);
		if (PlayerColField != null)
			PlayerColField.SetValue(player, col);
	}

	private static void InvokeFixedUpdate(PlayerController player)
	{
		FixedUpdateMethod.Invoke(player, null);
	}

	[UnitySetUp]
	public IEnumerator SetUp()
	{
		_playerGo = new GameObject();
		_playerGo.AddComponent<BoxCollider2D>();
		_rb = _playerGo.AddComponent<Rigidbody2D>();
		_rb.gravityScale = 0f;
		_player = _playerGo.AddComponent<PlayerController>();
		WirePlayerPhysics(_player, _rb, _playerGo.GetComponent<Collider2D>());
		yield return null;
	}

	[UnityTearDown]
	public IEnumerator TearDown()
	{
		if (_playerGo != null)
			Object.DestroyImmediate(_playerGo);
		yield return null;
	}

	[UnityTest]
	public IEnumerator MoveLeft_ThenFixedUpdate_VelocityXNegative()
	{
		_player.MoveLeft();
		InvokeFixedUpdate(_player);
		Assert.Less(_rb.velocity.x, 0f);
		yield return null;
	}

	[UnityTest]
	public IEnumerator MoveRight_ThenFixedUpdate_VelocityXPositive()
	{
		_player.MoveRight();
		InvokeFixedUpdate(_player);
		Assert.Greater(_rb.velocity.x, 0f);
		yield return null;
	}

	[UnityTest]
	public IEnumerator Stop_ThenFixedUpdate_VelocityXZero()
	{
		_player.MoveRight();
		InvokeFixedUpdate(_player);
		_player.Stop();
		Assert.AreEqual(0f, _rb.velocity.x, 0.01f);
		yield return null;
	}

	[UnityTest]
	public IEnumerator Jump_SetsPendingJump_NoImmediateVelocityChange()
	{
		float yBefore = _rb.velocity.y;
		_player.Jump();
		yield return null;
		Assert.AreEqual(yBefore, _rb.velocity.y);
	}

	[UnityTest]
	public IEnumerator SpawnItem_NullOrEmptyPrefabType_DoesNotThrow()
	{
		_player.SpawnItem(null, 0f, 0f);
		_player.SpawnItem("", 0f, 0f);
		yield return null;
	}

	[UnityTest]
	public IEnumerator SpawnItem_WithPrefabType_DoesNotThrow()
	{
		_player.SpawnItem("gem", 0f, 0f);
		yield return null;
	}
}
