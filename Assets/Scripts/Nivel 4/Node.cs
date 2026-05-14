using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Node> neightbourds;


    
    public bool hasTrap;
    Material mat;
    private void Start()
    {
        mat = GetComponent<Renderer>().material;
        GetNeightbourd(Vector3.right);
        GetNeightbourd(Vector3.left);
        GetNeightbourd(Vector3.forward);
        GetNeightbourd(Vector3.back);
    }
    private void Update()
    {
        if (hasTrap)
            mat.color = Color.red;
        else
            mat.color = Color.white;
    }
    void GetNeightbourd(Vector3 dir)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, 2.2f))
        {
            var node = hit.collider.GetComponent<Node>();
            if (node != null)
                neightbourds.Add(node);
        }
    }
}