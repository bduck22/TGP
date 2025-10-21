using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCircle : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = -30f; 
    void Start()
    {
        
    }

    void Update()
    {
        //if (!GameManager.Instance.isGameOver)
        //{
            transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
        //}    
    }
}
