using System.Collections.Generic;
using UnityEngine;

public class FurCollisionDetector : MonoBehaviour
{
    public List<string> touchingObjects = new List<string>();   // 현재 닿아있는 오브젝트들

    private void OnTriggerEnter(Collider other)
    {
        if (!touchingObjects.Contains(other.name))
            touchingObjects.Add(other.name);

        Debug.Log("들어온 오브젝트: " + other.name);

        Debug.Log("현재 닿아있는 목록:");
        foreach (var n in touchingObjects)
            Debug.Log(n);
    }

    private void OnTriggerExit(Collider other)
    {
        if (touchingObjects.Contains(other.name))
            touchingObjects.Remove(other.name);

        Debug.Log("빠져나간 오브젝트: " + other.name);

        Debug.Log("현재 닿아있는 목록:");
        foreach (var n in touchingObjects)
            Debug.Log(n);
    }
}
