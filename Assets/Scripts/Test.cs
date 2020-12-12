using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    GameObject pivot;
    Bounds bound;

    // Start is called before the first frame update
    void Start()
    {
        bound = gameObject.GetComponent<MeshRenderer>().bounds;

        transform.position = Vector3.zero;
        pivot = GameObject.Find("Pivot");
        pivot.transform.position = bound.center;
        transform.SetParent(pivot.transform);

        GameObject.Find("Test1").transform.position = new Vector3(bound.center.x + bound.extents.x, bound.center.y + bound.extents.y, bound.center.z + bound.extents.z);
        GameObject.Find("Test2").transform.position = new Vector3(bound.center.x + bound.extents.x, bound.center.y + bound.extents.y, bound.center.z - bound.extents.z);
        GameObject.Find("Test3").transform.position = new Vector3(bound.center.x + bound.extents.x, bound.center.y - bound.extents.y, bound.center.z + bound.extents.z);
        GameObject.Find("Test4").transform.position = new Vector3(bound.center.x + bound.extents.x, bound.center.y - bound.extents.y, bound.center.z - bound.extents.z);
        GameObject.Find("Test5").transform.position = new Vector3(bound.center.x - bound.extents.x, bound.center.y + bound.extents.y, bound.center.z + bound.extents.z);
        GameObject.Find("Test6").transform.position = new Vector3(bound.center.x - bound.extents.x, bound.center.y + bound.extents.y, bound.center.z - bound.extents.z);
        GameObject.Find("Test7").transform.position = new Vector3(bound.center.x - bound.extents.x, bound.center.y - bound.extents.y, bound.center.z + bound.extents.z);
        GameObject.Find("Test8").transform.position = new Vector3(bound.center.x - bound.extents.x, bound.center.y - bound.extents.y, bound.center.z - bound.extents.z);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
