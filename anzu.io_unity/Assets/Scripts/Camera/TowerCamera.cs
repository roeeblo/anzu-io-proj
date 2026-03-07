using UnityEngine;

public class TowerCamera : MonoBehaviour
{
	[SerializeField] private Transform _target;
	[SerializeField] private float _floorHeight = 5f;
	[SerializeField] private float _thresholdInFloor = 0.7f;
	[SerializeField] private float _smoothTime = 0.25f;
	[SerializeField] private bool _centerX = true;
	[SerializeField] private float _centerOffsetX = 0f;

	private Vector3 _velocity = Vector3.zero;
	private int _currentFloor;
	private float _cameraTargetY;

	private void Start()
	{
		if (_target == null)
		{
			var player = GameObject.FindGameObjectWithTag("Player");
			if (player != null) _target = player.transform;
		}
		if (_target != null)
		{
			_currentFloor = Mathf.FloorToInt(_target.position.y / _floorHeight);
			_cameraTargetY = _currentFloor * _floorHeight + _floorHeight * 0.5f;
		}
	}

	private void LateUpdate()
	{
		if (_target == null) return;

		float playerY = _target.position.y;
		int playerFloor = Mathf.FloorToInt(playerY / _floorHeight);
		float thresholdY = _currentFloor * _floorHeight + _floorHeight * _thresholdInFloor;

		if (playerFloor > _currentFloor && playerY >= thresholdY)
			_currentFloor = playerFloor;
		else if (playerFloor < _currentFloor)
			_currentFloor = playerFloor;

		_cameraTargetY = _currentFloor * _floorHeight + _floorHeight * 0.5f;

		float camX = _centerX ? _centerOffsetX : _target.position.x;
		Vector3 goal = new Vector3(camX, _cameraTargetY, transform.position.z);
		transform.position = Vector3.SmoothDamp(transform.position, goal, ref _velocity, _smoothTime);
	}
}
