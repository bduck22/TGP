using UnityEngine;

public class Blade : MonoBehaviour
{
    private Rigidbody2D rigidbody2D;
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.AddForce(-transform.right*50f, ForceMode2D.Impulse);
        Destroy(gameObject, 5);
    }
}
