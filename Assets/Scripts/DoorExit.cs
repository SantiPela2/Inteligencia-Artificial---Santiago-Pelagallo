using System.Collections; 
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorExit : MonoBehaviour
{
    [Header("Configuración de Nivel")]
    [SerializeField] private string nextLevelName;
    [SerializeField] private bool isLastLevel = false;

    [Header("Pantalla Final")]
    public GameObject pantallaDeVictoria;

    private void OnTriggerEnter(Collider other)
    {
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

        if (pantallaDeVictoria != null)
        {
            pantallaDeVictoria.SetActive(true);
            Time.timeScale = 0f; 
        }

        
        StartCoroutine(RutinaCierre());
    }

    
    private IEnumerator RutinaCierre()
    {
        
        yield return new WaitForSecondsRealtime(10f);

        Debug.Log("Apagando el juego...");

        
        Application.Quit();

        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}