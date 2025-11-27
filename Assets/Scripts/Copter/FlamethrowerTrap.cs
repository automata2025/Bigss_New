using UnityEngine;

public class FlamethrowerTrap : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] public float rotationSpeed = 60f;

    [Header("Flame Transform (your particle object)")]
    [SerializeField] public Transform flameObject;   
    [SerializeField] public Transform muzzleA;       

    private void Start()
    {
        // Move flame object to muzzle position
        if (flameObject != null && muzzleA != null)
        {
            flameObject.position = muzzleA.position;
            flameObject.rotation = muzzleA.rotation;
            flameObject.SetParent(muzzleA);
        }
    }

    private void Update()
    {
        // Rotate the trap
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}
