using TreeEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy5 : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Rigidbody playerRb; // Obligatorio para calcular el Pursue
    public Animator anim;
    public LineOfSight los;

    [Header("Steering")]
    public float maxSpeed = 4f;
    public float rotationSpeed = 10f;
    public float maxPredictionTime = 1.5f; // Cuántos segundos a futuro predice tu movimiento

    [Header("Antenas (Obstacle Avoidance)")]
    public float rayDistance = 2.5f; // Largo de los bigotes
    public LayerMask wallLayer; // ¡Importante! La capa de tus paredes

    [Header("Ataque")]
    public float attackRange = 2f;

    private ITreeNode _root;
    private bool isAttacking = false;

    // Variables de control para Wander
    private Vector3 currentWanderDirection;
    private float wanderTimer = 0f;
    public float wanderChangeInterval = 1.5f;

    void Start()
    {
        currentWanderDirection = transform.forward;
        ConstruirArbol();
    }

    void ConstruirArbol()
    {
        // 1. Nodos de Acción Básicos
        ITreeNode pursueNode = new ActionNode(AccionPursue);

        // 2. Nodo con probabilidades (Clase 5): 80% camina sin rumbo, 20% se queda quieto
        ITreeNode patrolRandomNode = new WeightedNode(
            new (float, System.Action)[]
            {
                (80f, AccionWander),
                (20f, AccionIdle)
            }
        );

        // 3. Nodo Pregunta: ¿El ninja está en mi campo de visión?
        _root = new QuestionNode(VeAlJugador, pursueNode, patrolRandomNode);
    }

    void Update()
    {
        if (player == null || isAttacking) return;

        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            Atacar();
            return;
        }

        // Ejecutamos la lógica de la IA en cada frame
        _root.Execute();
    }

    bool VeAlJugador()
    {
        return Vector3.Distance(transform.position, player.position) <= los.distance;
    }

    // --- ACCIONES DEL ÁRBOL ---

    void AccionPursue()
    {
        // Steering del profe: calcula hacia dónde correr para cortarte el paso
        Vector3 dir = SteeringBehaviours.Pursue(transform, player, playerRb, maxPredictionTime);

        // Le pasamos el vector a las antenas para que lo corrija si hay una pared
        dir = EvitarObstaculos(dir);

        Mover(dir);
    }

    void AccionWander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            // Steering del profe: gira aleatoriamente hasta 45 grados
            currentWanderDirection = SteeringBehaviours.Wander(currentWanderDirection, 45f);
            wanderTimer = wanderChangeInterval;
        }

        // Antenas: corrigen el rumbo si el Wander lo mandó directo contra un muro
        Vector3 dir = EvitarObstaculos(currentWanderDirection);
        currentWanderDirection = dir; // Actualiza el rumbo interno para no patinar

        Mover(dir);
    }

    void AccionIdle()
    {
        anim.SetFloat("Vel", 0f);
    }

    // --- MATEMÁTICA DE EVASIÓN (LAS ANTENAS MEJORADAS) ---
    Vector3 EvitarObstaculos(Vector3 dirOriginal)
    {
        Vector3 dirFinal = dirOriginal;
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up; // Las antenas salen del pecho

        bool detectoPared = false;

        // 1. Antena Izquierda (Si choca a la izquierda, dobla a la DERECHA fuerte)
        if (Physics.Raycast(origin, transform.forward - transform.right * 0.5f, out hit, rayDistance, wallLayer))
        {
            dirFinal += transform.right * 3f;
            detectoPared = true;
        }
        // 2. Antena Derecha (Si choca a la derecha, dobla a la IZQUIERDA fuerte)
        else if (Physics.Raycast(origin, transform.forward + transform.right * 0.5f, out hit, rayDistance, wallLayer))
        {
            dirFinal -= transform.right * 3f;
            detectoPared = true;
        }
        // 3. Antena Central (Peligro frontal directo, rebota usando la normal de la pared)
        else if (Physics.Raycast(origin, transform.forward, out hit, rayDistance, wallLayer))
        {
            dirFinal += hit.normal * 4f;
            detectoPared = true;
        }

        // Si detectó una pared y tuvo que esquivar, reseteamos el Wander para que 
        // no intente volver a girar hacia la pared inmediatamente por error.
        if (detectoPared)
        {
            wanderTimer = wanderChangeInterval;
        }

        // Dibuja las antenas en la vista "Scene" (Solo se ven en la pestaña Scene, no en Game)
        Debug.DrawRay(origin, transform.forward * rayDistance, Color.red);
        Debug.DrawRay(origin, (transform.forward + transform.right * 0.5f).normalized * rayDistance, Color.blue);
        Debug.DrawRay(origin, (transform.forward - transform.right * 0.5f).normalized * rayDistance, Color.blue);

        return dirFinal.normalized;
    }

    void Mover(Vector3 dir)
    {
        dir.y = 0;
        transform.position += dir * maxSpeed * Time.deltaTime;

        if (dir != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, dir.normalized, Time.deltaTime * rotationSpeed);
            anim.SetFloat("Vel", 1f);
        }
    }

    void Atacar()
    {
        isAttacking = true;
        anim.SetFloat("Vel", 0f);
        anim.SetTrigger("Punch");
        StartCoroutine(EsperarYReiniciar());
    }

    System.Collections.IEnumerator EsperarYReiniciar()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}