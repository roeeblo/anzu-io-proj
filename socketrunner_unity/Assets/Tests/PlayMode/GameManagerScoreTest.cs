using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class GameManagerScoreTest
{
	[TearDown]
	public void TearDown()
	{
		Time.timeScale = 1f;
	}

	[UnityTest]
	public IEnumerator AddScore_Reaches100_GameWonIsTrue()
	{
		var go = new GameObject();
		var gm = go.AddComponent<GameManager>();
		yield return null;

		Assert.AreEqual(0, gm.Score);
		Assert.IsFalse(gm.GameWon);

		gm.AddScore(50);
		yield return null;
		Assert.AreEqual(50, gm.Score);
		Assert.IsFalse(gm.GameWon);

		gm.AddScore(50);
		yield return null;
		Assert.AreEqual(100, gm.Score);
		Assert.IsTrue(gm.GameWon);

		Object.DestroyImmediate(go);
	}

	[UnityTest]
	public IEnumerator AddScore_StaysBelow100_GameWonIsFalse()
	{
		var go = new GameObject();
		var gm = go.AddComponent<GameManager>();
		yield return null;

		gm.AddScore(50);
		gm.AddScore(49);
		yield return null;
		Assert.AreEqual(99, gm.Score);
		Assert.IsFalse(gm.GameWon);

		Object.DestroyImmediate(go);
	}

	[UnityTest]
	public IEnumerator AddScore_SingleAddTo100_GameWonIsTrue()
	{
		var go = new GameObject();
		var gm = go.AddComponent<GameManager>();
		yield return null;

		gm.AddScore(100);
		yield return null;
		Assert.AreEqual(100, gm.Score);
		Assert.IsTrue(gm.GameWon);

		Object.DestroyImmediate(go);
	}
}
