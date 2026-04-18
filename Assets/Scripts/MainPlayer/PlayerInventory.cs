using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public bool hasKey = false;

    public void GrabKey()
    {
        hasKey = true;
        Debug.Log("¡Tenés la llave! Corré a la puerta.");
    }
}