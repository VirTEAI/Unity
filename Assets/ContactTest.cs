using UnityEngine;

public class ContactTest : MonoBehaviour
{
    [SerializeField] private GazeTracker gazeTracker;

    private void OnTriggerEnter(Collider other) // Este método ativa quando um objeto entra em contato com o trigger do objeto que possui este script
    {
        // if (other.CompareTag("Player")) // Verifica se o objeto que entrou em contato tem a tag "Player"
        // {
            Debug.Log("Player touched exit object");
            gazeTracker.EndSessionAndExit();
        // }
    }
}