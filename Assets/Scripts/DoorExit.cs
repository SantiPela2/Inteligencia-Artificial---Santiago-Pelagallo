using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorExit : MonoBehaviour
{
    [Header("Configuración de Nivel")]
    [SerializeField] private string nextLevelName; // Nombre de la siguiente escena
    [SerializeField] private bool isLastLevel = false; // Marcá esto solo en el Nivel 3

    private void OnTriggerEnter(Collider other)
    {
        // Buscamos el inventario en el Ninja
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();

        if (inventory != null)
        {
            if (inventory.hasKey)
            {
                if (isLastLevel)
                {
                    WinGame();
                }
                else
                {
                    NextLevel();
                }
            }
            else
            {
                Debug.Log("La puerta está bloqueada... Necesitás la llave.");
            }
        }
    }

    void NextLevel()
    {
        Debug.Log("¡Nivel Completado! Cargando " + nextLevelName);
        SceneManager.LoadScene(nextLevelName);
    }

    void WinGame()
    {
        Debug.Log("¡GANASTE LA PARTIDA! Escapaste de la prisión.");

        // Esto cierra el juego cuando lo exportes (.exe)
        Application.Quit();

        // Esto frena el Play en el editor de Unity
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}