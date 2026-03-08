using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class PlayerControllerTest
{
	private GameObject _playerGo;
	private PlayerController _player;
	private Rigidbody2D _rb;

	[UnitySetUp]
	public IEnumerator SetUp()
	{
		_playerGo = new GameObject();
		_playerGo.AddComponent<BoxCollider2D>();
		_rb = _playerGo.AddComponent<Rigidbody2D>();
		_rb.gravityScale = 0f;
		_player = _playerGo.AddComponent<PlayerController>();
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
		yield return new WaitForFixedUpdate();
		Assert.Less(_rb.velocity.x, 0f);
	}

	[UnityTest]
	public IEnumerator MoveRight_ThenFixedUpdate_VelocityXPositive()
	{
		_player.MoveRight();
		yield return new WaitForFixedUpdate();
		Assert.Greater(_rb.velocity.x, 0f);
	}

	[UnityTest]
	public IEnumerator Stop_ThenFixedUpdate_VelocityXZero()
	{
		_player.MoveRight();
		yield return new WaitForFixedUpdate();
		_player.Stop();
		yield return new WaitForFixedUpdate();
		Assert.AreEqual(0f, _rb.velocity.x, 0.01f);
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
