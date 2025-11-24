using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    public GameObject Mob;

    public float Spawntime;

    public float time;
    void Start()
    {
        
    }

    void Update()
    {
        if(time < Spawntime)
        {
            time += Time.deltaTime;
        }
        else
        {
            time = 0;
            Instantiate(Mob, transform.position, transform.rotation).GetComponent<Monster>().Player = GameManager2.instance.Player.transform;
        }
    }
}
