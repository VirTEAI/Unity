using UnityEngine;

public class DoorToOutside : MonoBehaviour
{
    // Referência ao TransitionManager.
    private TransitionManager transitionManager;

    // Evita que a porta seja usada várias vezes.
    private bool used;

     private void Start() // No Start, pegamos a referência para o TransitionManager usando a instância singleton.
    {
        transitionManager = TransitionManager.Instance;
    }

    // Esse método será chamado quando o jogador interagir com a porta.
    public void OnDoorInteracted()
    {
        // Se já foi usada, não faz nada.
        if (used) return;

        // Marca como usada.
        used = true;

        // Inicia a transição para fora.
        transitionManager.GoOutside();
    }
}