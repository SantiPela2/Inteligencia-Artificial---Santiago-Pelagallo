using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyEstratega : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Animator anim;
    public LineOfSight los;

    [Header("Steering (Arrive)")]
    public float maxSpeed = 3.5f;
    public float rotationSpeed = 10f;
    public float slowRadius = 2f;

    [Header("Ataque")]
    public float attackRange = 2f; 

    [Header("Pathfinding")]
    public LayerMask nodeLayer;
    private List<Node> currentPath = new List<Node>();
    private int currentPathIndex = 0;
    private float pathRecalculateTimer = 0f;

    private Node startNode;
    private Node targetNode;

    private bool isAttacking = false;

    void Update()
    {
        if (player == null || isAttacking) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            Atacar();
            return;
        }

        if (distanceToPlayer <= los.distance)
        {
            pathRecalculateTimer -= Time.deltaTime;

            // Recalcula la ruta cada 1 segundo 
            if (pathRecalculateTimer <= 0)
            {
                CalcularRutaAStar();
                pathRecalculateTimer = 1f;
            }
        }

        MoverPorRuta();
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

    void CalcularRutaAStar()
    {
        startNode = EncontrarNodoMasCercano(transform.position);
        targetNode = EncontrarNodoMasCercano(player.position);

        if (startNode != null && targetNode != null)
        {
            currentPath = AStar.Run(startNode, IsSatisfied, GetConnections, GetCosts, Heuristic);
            currentPathIndex = 0;
        }
    }

    void MoverPorRuta()
    {
        if (currentPath != null && currentPathIndex < currentPath.Count)
        {
            Vector3 targetPos = currentPath[currentPathIndex].transform.position;
            targetPos.y = transform.position.y;

            Vector3 dir;

            //Último nodo = Arrive para que frene
            if (currentPathIndex == currentPath.Count - 1)
            {
                dir = SteeringBehaviours.Arrive(transform, targetPos, slowRadius);
            }
            // Nodos intermedios = Seek
            else
            {
                dir = SteeringBehaviours.Seek(transform, targetPos);
            }

            transform.position += dir * maxSpeed * Time.deltaTime;

            if (dir != Vector3.zero)
            {
                transform.forward = Vector3.Lerp(transform.forward, dir.normalized, Time.deltaTime * rotationSpeed);

                
                float animSpeed = (currentPathIndex == currentPath.Count - 1) ? dir.magnitude : 1f;
                anim.SetFloat("Vel", animSpeed);
            }

            
            // No da más vueltas el enemigo
            if (Vector3.Distance(transform.position, targetPos) < 1f)
            {
                currentPathIndex++;
            }
        }
        else
        {
            anim.SetFloat("Vel", 0f);
        }
    }

    bool IsSatisfied(Node node) { return node == targetNode; }
    List<Node> GetConnections(Node node) { return node.neightbourds; }
    float GetCosts(Node a, Node b) { return b.hasTrap ? 100f : 1f; }
    float Heuristic(Node node) { return Vector3.Distance(node.transform.position, targetNode.transform.position); }

    Node EncontrarNodoMasCercano(Vector3 posicion)
    {
        Collider[] colliders = Physics.OverlapSphere(posicion, 5f, nodeLayer);
        Node closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider col in colliders)
        {
            float dist = Vector3.Distance(posicion, col.transform.position);
            if (dist < minDistance)
            {
                closest = col.GetComponent<Node>();
                minDistance = dist;
            }
        }
        return closest;
    }
}