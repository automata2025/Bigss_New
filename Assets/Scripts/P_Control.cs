using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P_Control : MonoBehaviour
{
    [SerializeField] private int crabAmounts = 4;

    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float drag = 0.98f;
    [SerializeField] private float steerAngle = 20f;
    [SerializeField] private float Traction = 1f;

    [SerializeField] private float miniCrabSpeed = 5f;
    [SerializeField] private int gap = 10;
    [SerializeField] private GameObject crabPrefab;
    
    private List<GameObject> crabPart = new List<GameObject> ();
    private List<Vector3> positionsHistory = new List<Vector3> ();


    Vector3 moveForce;

    private void Start()
    {
        for (int i = 0; i < crabAmounts; i++)
        {
            SpawnCrab();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Moving
        moveForce += transform.forward * moveSpeed * Input.GetAxis("Vertical") * Time.deltaTime;
        transform.position += moveForce * Time.deltaTime;

        //Drag
        float steerInput = Input.GetAxis("Horizontal");
        transform.Rotate(Vector3.up * steerInput, moveForce.magnitude * steerAngle * Time.deltaTime);

        //Tracting
        Debug.DrawRay(transform.position, moveForce.normalized * 3);
        Debug.DrawRay(transform.position, transform.forward * 3, Color.blue);
        moveForce = Vector3.Lerp(moveForce.normalized, transform.forward, Traction * Time.deltaTime) * moveForce.magnitude;

        //Store position History
        positionsHistory.Insert(0, transform.position);

        //Move Body Part
        int index = 0;
        foreach (var body in crabPart)
        {
            Vector3 point = positionsHistory[Mathf.Min(index * gap , positionsHistory.Count - 1)];
            Vector3 moveDireaction = point - body.transform.position;
            body.transform.position += moveDireaction * miniCrabSpeed * Time.deltaTime;
            body.transform.LookAt(point);
        }
    }

    private void SpawnCrab () 
    { 
        GameObject crab = Instantiate(crabPrefab);
        crabPart.Add(crab);
    }
}
