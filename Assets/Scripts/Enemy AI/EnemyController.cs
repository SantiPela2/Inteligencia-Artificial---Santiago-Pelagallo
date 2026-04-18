using UnityEngine;
using UnityEngine.SceneManagement; // Con esto reinicio el nivel

public class EnemyController : MonoBehaviour
{
    LineOfSight los;
    [SerializeField] Transform player;
    [SerializeField] float speed = 2f;
    [SerializeField] float attackRange = 1.5f;

    public EnemyState currentState = EnemyState.Patrol;
    Animator anim;

    private void Awake()
    {
        los = GetComponent<LineOfSight>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // Lógica de Percepción (LoS)
        bool canSeePlayer = los.CheckRange(transform, player) &&
                            los.CheckAngle(transform, player) &&
                            los.CheckObstacle(transform, player);

        // Selección de Estados
        float dist = Vector3.Distance(transform.position, player.position);

        if (canSeePlayer && dist <= attackRange) currentState = EnemyState.Attack;
        else if (canSeePlayer) currentState = EnemyState.Pursuit;
        else currentState = EnemyState.Patrol;

        ExecuteState(dist);
    }

    void ExecuteState(float dist)
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                
                anim.SetFloat("Vel", 0);
                break;

            case EnemyState.Pursuit:
                anim.SetFloat("Vel", 1); // Animación caminar
                LookAtPlayer();
                transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
                break;

            case EnemyState.Attack:
                
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("EnemyPunch"))
                {
                    anim.SetTrigger("Punch");
                    Debug.Log("¡Te atraparon!");
                    
                    Invoke("RestartLevel", 0.5f);
                }
                break;
        }
    }

    void LookAtPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        transform.forward = dir;
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}