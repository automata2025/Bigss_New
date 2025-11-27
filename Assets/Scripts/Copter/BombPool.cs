using System.Collections.Generic;
using UnityEngine;

public class BombPool : MonoBehaviour
{
    [Header("Pooling Settings")]
    public GameObject bombPrefab;
    public int poolSize = 20;
    public Transform poolParent;

    private Queue<GameObject> poolQueue = new Queue<GameObject>();

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bomb = Instantiate(bombPrefab, Vector3.zero, Quaternion.identity, poolParent);
            bomb.SetActive(false);
            poolQueue.Enqueue(bomb);
        }
    }

    public GameObject GetBomb(Vector3 position, Quaternion rotation)
    {
        GameObject bomb = poolQueue.Count > 0 ? poolQueue.Dequeue() : Instantiate(bombPrefab, poolParent);
        bomb.transform.SetPositionAndRotation(position, rotation);
        bomb.SetActive(true);

        Bomb bombScript = bomb.GetComponent<Bomb>();
        if (bombScript != null) bombScript.AssignPool(this);

        return bomb;
    }

    public void ReturnBomb(GameObject bomb)
    {
        bomb.SetActive(false);
        poolQueue.Enqueue(bomb);
    }
}
