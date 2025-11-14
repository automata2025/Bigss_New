using UnityEngine;

public class Bomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private float _deactivateDelay = 0.2f;
    public float DeactivateDelay => _deactivateDelay;

    private Rigidbody _rigidbody;
    private BombPool _pool;

    public Rigidbody Rigidbody => _rigidbody;
    public BombPool Pool => _pool;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;

        if (TryGetComponent(out Renderer rend)) rend.enabled = true;
        if (TryGetComponent(out Collider col)) col.enabled = true;
    }

    public void AssignPool(BombPool pool)
    {
        _pool = pool;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Disappear();
        }
    }

    private void Disappear()
    {
        if (TryGetComponent(out Renderer rend)) rend.enabled = false;
        if (TryGetComponent(out Collider col)) col.enabled = false;

        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.isKinematic = true;

        if (_pool != null)
            _pool.ReturnBomb(gameObject);
        else
            gameObject.SetActive(false);
    }
}
