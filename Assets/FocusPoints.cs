using UnityEngine; // Importa as ferramentas básicas da Unity

public class FocusPoints : MonoBehaviour // Cria um componente que pode ser colocado em qualquer objeto da cena
{
    [Header("Stable ID (DO NOT CHANGE AFTER DEPLOY)")]
    public string pointId; // ID único e fixo do objeto. Serve para identificar esse foco nos dados

    [Header("Optional category")]
    public string category; // Categoria opcional do objeto, por exemplo: "window", "door", "person"
}