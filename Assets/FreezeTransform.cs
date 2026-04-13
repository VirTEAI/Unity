using UnityEngine; // Importa as funções principais da Unity

public class FreezeTransform : MonoBehaviour // Cria um componente que pode ser colocado em qualquer objeto da cena
{
    private Vector3 lockedPosition; // Guarda a posição inicial do objeto
    private Quaternion lockedRotation; // Guarda a rotação inicial do objeto
    public bool freeze = true; // Diz se o objeto deve ficar travado ou não

    void Start() // Função que roda uma vez quando o objeto começa a funcionar
    {
        lockedPosition = transform.position; // Salva a posição atual do objeto
        lockedRotation = transform.rotation; // Salva a rotação atual do objeto
    }

    void LateUpdate() // Função que roda depois do Update de todos os outros scripts
    {
        if (!freeze) return; // Se freeze for falso, não faz nada e deixa o objeto livre

        transform.position = lockedPosition; // Volta o objeto para a posição travada
        transform.rotation = lockedRotation; // Volta o objeto para a rotação travada
    }

    public void Unlock() // Função pública que destrava o objeto
    {
        freeze = false; // Muda o estado para destravado
    }
}