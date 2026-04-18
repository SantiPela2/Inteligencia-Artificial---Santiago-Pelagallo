using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyStalker : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Animator anim;

    [Header("Configuración")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float attackRange = 2f;

    private NavMeshAgent agent;
    private bool isDeadly = true;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        agent.speed = speed;
    }

    void Update()
    {
        if (!player || !isDeadly) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (IsPlayerLookingAtMe())
        {
            // ESTADO: IDLE
            agent.isStopped = true;
            anim.SetFloat("Vel", 0f);
        }
        else
        {
            // ESTADO: WALK
            agent.isStopped = false;
            agent.SetDestination(player.position);
            anim.SetFloat("Vel", 1f);
        }

        // ESTADO: ATTACK
        if (dist <= attackRange)
        {
            Attack();
        }
    }

    bool IsPlayerLookingAtMe()
    {
        Vector3 dirToMe = (transform.position - player.position).normalized;
        float dot = Vector3.Dot(player.forward, dirToMe);
        return dot > 0.6f;
    }

    void Attack()
    {
        isDeadly = false;
        agent.isStopped = true;
        anim.SetFloat("Vel", 0f); // Frenamos la animación de caminar para pegar
        anim.SetTrigger("Punch");

        Invoke("RestartLevel", 0.3f);
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}