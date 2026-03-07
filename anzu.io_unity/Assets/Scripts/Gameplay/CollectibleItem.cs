using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollectibleItem : MonoBehaviour
{
	[SerializeField] private float _collectRadius = 2f;

	private void Awake()
	{
		Collider2D[] cols = GetComponentsInChildren<Collider2D>(true);
		for (int i = 0; i < cols.Length; i++)
			cols[i].enabled = false;
	}

	private void Update()
	{
		GameObject player = GameObject.FindWithTag("Player");
		if (player == null) return;

		float dist = Vector2.Distance(transform.position, player.transform.position);
		if (dist > _collectRadius) return;

		GameManager gm = GameManager.Instance ?? Object.FindObjectOfType<GameManager>();
		if (gm != null)
			gm.AddScore(5);

		Destroy(gameObject);
	}
}
