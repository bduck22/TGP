using UnityEngine;

public class Gig : MonoBehaviour
{
    private Rigidbody2D rigidbody2D;
    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segmentCount;
    }

    private LineRenderer lineRenderer;

    public Transform targetObject;

    [Range(0.01f, 1f)]
    public float sagFactor = 0.5f;

    public int segmentCount = 50;

    private void Update()
    {
        if (rigidbody2D && rigidbody2D.gravityScale != 0)
        {
            transform.LookAt((Vector2)transform.position + rigidbody2D.linearVelocity.normalized);
        }

        if (targetObject == null) return;

        DrawChainCurve(transform.position, targetObject.position);
    }

    public void Shoot()
    {
        rigidbody2D.linearVelocity = Vector2.zero;
        rigidbody2D.AddForce(transform.forward * 35f, ForceMode2D.Impulse);
        transform.LookAt((Vector2)transform.position + rigidbody2D.linearVelocity.normalized);
    }

    private void DrawChainCurve(Vector3 startPoint, Vector3 endPoint)
    {
        Vector3 midPoint = (startPoint + endPoint) / 2f;

        float distance = Vector3.Distance(startPoint, endPoint);

        float sagAmount = distance * sagFactor;

        Vector3 controlPoint = midPoint + Vector3.down * sagAmount;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = (float)i / (segmentCount - 1);

            Vector3 position = Mathf.Pow((1 - t), 2) * startPoint +
                               2 * (1 - t) * t * controlPoint +
                               Mathf.Pow(t, 2) * endPoint;

            lineRenderer.SetPosition(i, position);
        }
    }


    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.CompareTag("Enemy"))
    //    {
    //        transform.parent = collision.transform.parent;
    //        rigidbody2D.gravityScale = 0;
    //        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
    //    }
    //}
}