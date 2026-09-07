using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    public float destroyTime = 1f;
    public GameObject attackPerfab;
    public Transform player;
    void Start()
    {
        Destroy(gameObject, destroyTime);
        player = GameObject.Find("Player").transform;
    }
    public void Skill1()
    {
        GameObject go = Instantiate(attackPerfab, transform.position, transform.rotation);
        go.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        go.GetComponent<AttackPerfab>().Init(true, 20, player);
    }
    public void Skill2()
    {
        GameObject go = Instantiate(attackPerfab, transform.position, transform.rotation);
        go.transform.localScale = new Vector3(3f, 3f, 3f);
        go.GetComponent<AttackPerfab>().Init(true, 20, player);
    }
    public void Skill3()
    {
        GameObject go = Instantiate(attackPerfab, transform.position, transform.rotation);
        go.transform.localScale = new Vector3(10f, 10f, 10f);
        go.GetComponent<AttackPerfab>().Init(true, 30, player);
    }

}
