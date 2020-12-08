using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    GameObject pivot;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.position = Vector3.zero;
        pivot = GameObject.Find("Pivot");
        pivot.transform.position = gameObject.GetComponent<MeshRenderer>().bounds.center;
        gameObject.transform.SetParent(pivot.transform);
    }

    // Update is called once per frame
    void Update()
    {
        pivot.transform.Rotate(0.1f, 0, 0);
    }
}
