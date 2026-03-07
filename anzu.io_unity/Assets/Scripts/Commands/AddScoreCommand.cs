using UnityEngine;

public class AddScoreCommand : ICommand
{
	private readonly int _amount;

	public AddScoreCommand(int amount)
	{
		_amount = amount <= 0 ? 1 : amount;
	}

	public void Execute()
	{
		GameManager gm = GameManager.Instance ?? UnityEngine.Object.FindObjectOfType<GameManager>();
		if (gm == null)
		{
			Debug.LogError("[AddScore] No GameManager in scene");
			return;
		}
		gm.AddScore(_amount);
	}
}
