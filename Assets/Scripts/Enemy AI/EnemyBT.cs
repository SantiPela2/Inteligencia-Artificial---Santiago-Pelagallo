using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyBT : MonoBehaviour
{
    private LineOfSight los;
    private Animator anim;
    [SerializeField] private Transform player;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float attackRange = 2.5f;

    void Awake()
    {
        los = GetComponent<LineOfSight>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // BT
        // Prioridad Atacar Perseguir Patrullar
        if (Node_Attack()) return;
        if (Node_Pursuit()) return;
        Node_Patrol();
    }

    //Nodos

    bool Node_Attack()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        // Condición para entrar en este nodo
        if (dist <= attackRange && los.CheckRange(transform, player) && los.CheckObstacle(transform, player))
        {
            anim.SetFloat("Vel", 0);
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("EnemyPunch"))
            {
                anim.SetTrigger("Punch");
                Invoke("RestartLevel", 1f);
            }
            return true; // se ejecutó bien
        }
        return false; // falló entonces pasa al siguiente nodo
    }

    bool Node_Pursuit()
    {
        // Condición: ¿Lo veo?
        if (los.CheckRange(transform, player) && los.CheckAngle(transform, player) && los.CheckObstacle(transform, player))
        {
            anim.SetFloat("Vel", 1);
            LookAtPlayer();
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            return true;
        }
        return false;
    }

    void Node_Patrol()
    {
        // Si todo lo anterior falla, el árbol va aca
        anim.SetFloat("Vel", 0);
        
    }

    
    void LookAtPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        transform.forward = dir;
    }

    void RestartLevel() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
}