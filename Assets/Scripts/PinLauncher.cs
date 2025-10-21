using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinLauncher : MonoBehaviour
{
    [SerializeField] private GameObject pinObject;
    [SerializeField] private Pin _currPin;
    void Start()
    {
        //PreparePin();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && _currPin != null && !GameManager.Instance.isGameOver)
        {
            _currPin.Launch();
            _currPin = null;
        }
    }

    //void PreparePin()
    //{
    //    if (!GameManager.Instance.isGameOver)
    //    {
    //        GameObject pin = Instantiate(pinObject, transform.position, Quaternion.identity);
    //        _currPin = pin.GetComponent<Pin>();
    //    }
    //}
}
