using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class IntUnityEvent : UnityEvent<int> { }

[DefaultExecutionOrder(-200)]
public class GameManager : MonoBehaviour
{
	public const int GoalScore = 100;

	public static GameManager Instance { get; private set; }

	[SerializeField] private int _score;
	[SerializeField] private bool _gameWon;

	public int Score => _score;
	public bool GameWon => _gameWon;

	public IntUnityEvent OnScoreChanged;
	public UnityEvent OnGameWon;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		_score = 0;
		_gameWon = false;
		if (OnScoreChanged == null) OnScoreChanged = new IntUnityEvent();
		if (OnGameWon == null) OnGameWon = new UnityEvent();
	}

	public void AddScore(int points)
	{
		if (_gameWon) return;
		_score = Mathf.Min(_score + points, GoalScore);
		OnScoreChanged?.Invoke(_score);
		if (_score >= GoalScore)
		{
			_gameWon = true;
			Time.timeScale = 0f;
			OnGameWon?.Invoke();
		}
	}

	public void ResetGame()
	{
		_score = 0;
		_gameWon = false;
		Time.timeScale = 1f;
		OnScoreChanged?.Invoke(_score);
	}
}
