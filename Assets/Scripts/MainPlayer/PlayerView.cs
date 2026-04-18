using UnityEngine;

public class PlayerView : MonoBehaviour
{
    [SerializeField]Animator anim;
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        anim.SetFloat("Vel", rb.linearVelocity.magnitude);
    }
}
