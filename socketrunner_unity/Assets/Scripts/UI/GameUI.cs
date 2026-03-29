using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
	[SerializeField] private Text _scoreText;
	[SerializeField] private GameObject _winPanel;
	private int _lastScore = -1;

	private const string WinPanelTag = "WinMenu";

	private void Start()
	{
		if (_winPanel == null && !string.IsNullOrEmpty(WinPanelTag))
			_winPanel = GameObject.FindGameObjectWithTag(WinPanelTag);
		if (_winPanel != null)
			_winPanel.SetActive(false);
		if (_scoreText != null)
		{
			_scoreText.fontSize = 48;
			RectTransform rt = _scoreText.rectTransform;
			rt.anchorMin = new Vector2(0f, 1f);
			rt.anchorMax = new Vector2(0f, 1f);
			rt.pivot = new Vector2(0f, 1f);
			rt.anchoredPosition = new Vector2(20f, -20f);
			rt.sizeDelta = new Vector2(380f, 80f);

			Outline outline = _scoreText.GetComponent<Outline>();
			if (outline == null)
				outline = _scoreText.gameObject.AddComponent<Outline>();
			outline.effectColor = Color.black;
			outline.effectDistance = new Vector2(2f, 2f);
		}
	}

	private void OnEnable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnScoreChanged.AddListener(UpdateScore);
			GameManager.Instance.OnGameWon.AddListener(ShowWin);
		}
		UpdateScore(GameManager.Instance != null ? GameManager.Instance.Score : 0);
		if (_winPanel != null) _winPanel.SetActive(false);
	}

	private void OnDisable()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.OnScoreChanged.RemoveListener(UpdateScore);
			GameManager.Instance.OnGameWon.RemoveListener(ShowWin);
		}
	}

	private void Update()
	{
		if (GameManager.Instance == null || _scoreText == null) return;
		int s = GameManager.Instance.Score;
		if (s != _lastScore)
		{
			_lastScore = s;
			_scoreText.text = string.Format("Score: {0} / {1}", s, GameManager.GoalScore);
		}
		if (GameManager.Instance.GameWon && _winPanel != null)
		{
			_winPanel.SetActive(true);
			_winPanel.transform.SetAsLastSibling();
		}
	}

	private void UpdateScore(int score)
	{
		if (_scoreText != null)
			_scoreText.text = string.Format("Score: {0} / {1}", score, GameManager.GoalScore);
	}

	private async void ShowWin()
	{
		Time.timeScale = 0f;
		if (_winPanel != null)
		{
			_winPanel.SetActive(true);
			_winPanel.transform.SetAsLastSibling();
			Canvas can = _winPanel.GetComponentInParent<Canvas>();
			if (can != null)
				can.sortingOrder = 100;
		}
		NetworkProvider np = UnityEngine.Object.FindObjectOfType<NetworkProvider>();
		if (np != null)
			await np.SendAsync("{\"type\":\"GAME_OVER\",\"payload\":{\"message\":\"Game ended.\"}}");
	}
}
