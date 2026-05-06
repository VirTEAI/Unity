using UnityEngine;

public class PersistentLoadingUI : MonoBehaviour
{
    private static PersistentLoadingUI instance; // Singleton para garantir que só exista uma instância do PersistentLoadingUI

    public static PersistentLoadingUI Instance // Propriedade pública para acessar a instância do PersistentLoadingUI
    {
        get
        {
            if (instance == null) // Verifica se a instância é nula, o que pode acontecer se for acessada antes do Awake ser chamado
            {
                Debug.LogError("PersistentLoadingUI.Instance was accessed before Awake. Ensure a PersistentLoadingUI object exists in the scene.");
            }
            return instance;
        }
    }

    public GameObject canvas; // Referência ao GameObject do canvas que contém a UI de carregamento

    void Awake() // Função que roda quando o objeto é criado
    {
        if (instance != null) // Se já existe uma instância, destrói esta nova para evitar duplicação
        {
            Destroy(gameObject);
            return;
        }

        instance = this; // Define esta instância como a única instância do PersistentLoadingUI
        DontDestroyOnLoad(gameObject); // Faz com que este objeto não seja destruído ao carregar uma nova cena, mantendo a UI de carregamento persistente
    }

    public void Show() // Método para mostrar a UI de carregamento, ativando o canvas
    {
        canvas.SetActive(true);
    }

    public void Hide() // Método para esconder a UI de carregamento, desativando o canvas
    {
        canvas.SetActive(false);
    }
}