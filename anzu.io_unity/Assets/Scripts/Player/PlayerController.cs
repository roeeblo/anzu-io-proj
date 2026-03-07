using UnityEngine;

[System.Serializable]
public class SpawnableEntry
{
	public string prefabType;
	public GameObject prefab;
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
	private const string AnimSpeed = "Speed";
	private const string AnimGrounded = "Grounded";
	private const string AnimJump = "Jump";

	[SerializeField] private float _moveSpeed = 12f;
	[SerializeField] private float _jumpForce = 10f;
	[SerializeField] private LayerMask _groundMask = ~0;
	[SerializeField] private float _groundCheckRadius = 0.5f;
	[SerializeField] private Transform _groundCheck;
	[SerializeField] private float _groundCheckOffsetY = -1f;
	[SerializeField] private float _wallCheckDistance = 0.15f;
	[SerializeField] private SpawnableEntry[] _spawnables;
	[SerializeField] private Animator _animator;
	[SerializeField] private SpriteRenderer _spriteRenderer;

	private Rigidbody2D _rb;
	private Collider2D _col;
	private bool _hasGroundedBool;
	private int _moveDirection;
	private bool _pendingJump;
	private bool _jumpConsumed;
	private bool _wasGroundedLastFrame;

	private void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();
		_rb.drag = 0f;
		_col = GetComponent<Collider2D>();

		if (_col != null)
		{
			PhysicsMaterial2D noFriction = new PhysicsMaterial2D();
			noFriction.friction = 0f;
			_col.sharedMaterial = noFriction;
		}

		if (_animator == null) _animator = GetComponent<Animator>();
		if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

		if (_animator != null)
		{
			foreach (AnimatorControllerParameter p in _animator.parameters)
			{
				if (p.name == AnimGrounded && p.type == AnimatorControllerParameterType.Bool)
				{
					_hasGroundedBool = true;
					break;
				}
			}
		}

		_wasGroundedLastFrame = IsGrounded();
		_jumpConsumed = false;
	}

	private void Update()
	{
		bool grounded = IsGrounded();

		if (_wasGroundedLastFrame && !grounded)
		{
			_jumpConsumed = true;
		}

		if (!_wasGroundedLastFrame && grounded)
		{
			_jumpConsumed = false;
		}

		if (_pendingJump && grounded && !_jumpConsumed)
		{
			_pendingJump = false;
			_jumpConsumed = true;
			_rb.velocity = new Vector2(_rb.velocity.x, 0f);
			_rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);

			if (_animator != null)
				_animator.SetTrigger(AnimJump);
		}
		else if (_pendingJump && !grounded)
		{
			_pendingJump = false;
		}

		if (_animator != null)
		{
			if (_hasGroundedBool)
				_animator.SetBool(AnimGrounded, grounded);

			_animator.SetFloat(AnimSpeed, Mathf.Abs(_rb.velocity.x));
		}

		_wasGroundedLastFrame = grounded;
	}

	private void FixedUpdate()
	{
		if (_moveDirection != 0 && IsBlockedInDirection(_moveDirection))
			_moveDirection = 0;

		_rb.velocity = new Vector2(_moveDirection * _moveSpeed, _rb.velocity.y);
	}

	private bool IsBlockedInDirection(int direction)
	{
		if (_col == null || direction == 0) return false;

		float dir = direction > 0 ? 1f : -1f;
		float frontX = dir > 0 ? _col.bounds.max.x + 0.02f : _col.bounds.min.x - 0.02f;
		Vector2 origin = new Vector2(frontX, _col.bounds.center.y);
		RaycastHit2D hit = Physics2D.Raycast(origin, new Vector2(dir, 0f), _wallCheckDistance, _groundMask);

		return hit.collider != null && hit.collider.gameObject != gameObject;
	}

	private bool IsGrounded()
	{
		if (_rb.velocity.y > 0.5f)
			return false;

		Vector2 origin;

		if (_groundCheck != null && _groundCheck.IsChildOf(transform))
			origin = _groundCheck.position;
		else if (_col != null)
			origin = new Vector2(transform.position.x, _col.bounds.min.y - 0.1f);
		else
			origin = (Vector2)transform.position + new Vector2(0f, _groundCheckOffsetY);

		Collider2D overlap = Physics2D.OverlapCircle(origin, _groundCheckRadius, _groundMask);
		if (overlap != null && overlap.gameObject != gameObject)
			return true;

		RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.35f, _groundMask);
		return hit.collider != null && hit.collider.gameObject != gameObject;
	}

	public void MoveLeft()
	{
		_moveDirection = -1;
		if (_spriteRenderer != null) _spriteRenderer.flipX = true;
	}

	public void MoveRight()
	{
		_moveDirection = 1;
		if (_spriteRenderer != null) _spriteRenderer.flipX = false;
	}

	public void Jump()
	{
		_pendingJump = true;
	}

	public void Stop()
	{
		_moveDirection = 0;
		_rb.velocity = new Vector2(0f, _rb.velocity.y);

		if (_animator != null)
		{
			_animator.SetFloat(AnimSpeed, 0f);
			_animator.Play("Idle", 0, 0f);
		}
	}

	public void SpawnItem(string prefabType, float x, float y)
	{
		if (_spawnables == null || string.IsNullOrEmpty(prefabType)) return;

		string key = prefabType.Trim().ToLowerInvariant();

		foreach (SpawnableEntry e in _spawnables)
		{
			if (e == null || e.prefab == null) continue;

			string eType = (e.prefabType ?? "").Trim().ToLowerInvariant();

			if (eType == key)
			{
				Vector3 pos = new Vector3(x, y, 0f);
				Instantiate(e.prefab, pos, Quaternion.identity);
				return;
			}
		}
	}
}