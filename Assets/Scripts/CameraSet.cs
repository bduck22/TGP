using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraSet : MonoBehaviour
{
    public static CameraSet Instance;
    private void Awake()
    {
        Instance = this;
    }

    public Transform Target;
    public bool Follow;
    public float Speed;

    public Volume Volume;

    public Vignette vig;
    public ColorAdjustments cadj;

    public Vector2 NotMi;
    public Vector2 NotPl;
    void Start()
    {
        Volume = GetComponent<Volume>();
        Volume.profile.TryGet<Vignette>(out vig);
        Volume.profile.TryGet<ColorAdjustments>(out cadj);
    }
    void Update()
    {
        if (Follow && Target)
        {
            float x = transform.position.x;
            float y = Target.position.y - 2f;
            if (transform.position.x > NotMi.x && transform.position.x < NotPl.x)
            {
                x = Target.position.x;
            }

            if (y < NotMi.y)
            {
                y = NotMi.y;
            }

            transform.position = Vector3.Lerp(transform.position, new Vector3(x, y, transform.position.z), Speed * Time.deltaTime);

            if (transform.position.x < NotMi.x)
            {
                transform.position = new Vector3(NotMi.x, transform.position.y, transform.position.z);
            }
            if (transform.position.x > NotPl.x)
            {
                transform.position = new Vector3(NotPl.x, transform.position.y, transform.position.z);
            }
            if (transform.position.y > NotPl.y)
            {
                transform.position = new Vector3(transform.position.x, NotPl.y, transform.position.z);
            }
        }
    }

}
