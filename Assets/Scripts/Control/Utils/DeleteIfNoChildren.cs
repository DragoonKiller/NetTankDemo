using System;
using UnityEngine;
using Utils;

public class DeleteIfNoChildren : MonoBehaviour
{
    void Update()
    {
        if(this.transform.childCount == 0) Destroy(this.gameObject);
    }
}
