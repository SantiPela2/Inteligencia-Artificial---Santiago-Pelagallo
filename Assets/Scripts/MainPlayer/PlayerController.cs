using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerModel model;

    void Start()
    {
        model = GetComponent<PlayerModel>();
    }

    void Update()
    {
        
        float moveH = Input.GetAxisRaw("Horizontal");
        float moveV = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(moveH, 0, moveV).normalized;

        model.Move(direction);
        model.Rotate(direction);
    }
}