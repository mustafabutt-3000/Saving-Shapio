using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RePosition : MonoBehaviour
{
    void Start()
    {
        GameObject sceneChanger = GameObject.Find("SceneChanger");
        if (sceneChanger != null)
        {
            sceneChanger.transform.position = transform.position;
        }
    }
}
