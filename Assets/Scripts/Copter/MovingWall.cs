using UnityEngine;

public class MovingWall : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool isActive = false;

    public float Speed
    {
        get => speed;
        set => speed = Mathf.Max(0, value);
    }

    void Update()
    {
        if (!isActive) return;
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    public void Activate()
    {
        isActive = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            Debug.Log("Detected wall — stopping movement.");
            isActive = false;
        }
    }
}
