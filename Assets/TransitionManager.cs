using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    // Instância única do gerenciador.
    // Isso permite acessar este script de qualquer outro lugar com TransitionManager.Instance.
    public static TransitionManager Instance;

    [Header("UI")]
    // Referência para o Canvas da tela de loading.
    [SerializeField] private Canvas loadingCanvas;

    // Referência opcional para controlar transparência e bloqueio de cliques do loading.
    [SerializeField] private CanvasGroup loadingGroup;

    [Header("Player")]
    // Referência para o XR Origin do jogador.
    [SerializeField] private Transform xrOrigin;

    [Header("Scenes")]
    // Nome da cena da casa.
    [SerializeField] private string houseSceneName = "House";

    // Nome da cena de fora.
    [SerializeField] private string outsideSceneName = "Outside";

    // Tag do objeto spawn da cena de fora.
    [SerializeField] private string outsideSpawnTag = "OutsideSpawn";

    [Header("Optional")]
    // Lista de componentes que devem ser desativados durante o loading.
    // Exemplo: locomotion, input, interações da porta, etc.
    [SerializeField] private Behaviour[] disableWhileLoading;

    // Variável para impedir que o jogador dispare a transição mais de uma vez.
    private bool isTransitioning;

    private void Awake()
    {
        // Se já existir uma instância deste gerenciador, destrói esta nova.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Define esta instância como a principal.
        Instance = this;

        // Faz com que este objeto não seja destruído ao trocar de cena.
        // Isso é importante porque o loading e o XR Origin precisam continuar existindo.
        DontDestroyOnLoad(gameObject);

        // Começa com a tela de loading escondida.
        HideLoading();
    }

    // Método público que será chamado quando o jogador interagir com a porta.
    public void GoOutside()
    {
        // Só inicia se ainda não estiver em transição.
        if (!isTransitioning)
            StartCoroutine(GoOutsideRoutine());
    }

    // Coroutine porque o carregamento de cena é assíncrono.
    private IEnumerator GoOutsideRoutine()
    {
        // Marca que a transição começou.
        isTransitioning = true;

        // Mostra a tela preta de loading.
        ShowLoading();

        // Desativa controles, interações ou qualquer coisa que não deva funcionar durante o carregamento.
        foreach (var b in disableWhileLoading)
        {
            if (b != null)
                b.enabled = false;
        }

        // Espera 1 frame para garantir que a UI tenha tempo de aparecer na tela.
        yield return null;

        // Carrega a cena Outside de forma assíncrona e aditiva.
        // "Additive" significa que ela entra junto com a cena atual.
        AsyncOperation loadOutside = SceneManager.LoadSceneAsync(outsideSceneName, LoadSceneMode.Additive);

        // Espera até a cena terminar de carregar.
        while (!loadOutside.isDone)
            yield return null;

        // Procura o objeto de spawn na cena de fora usando a tag definida.
        GameObject spawn = GameObject.FindWithTag(outsideSpawnTag);

        // Se encontrou o spawn e o XR Origin existe, move o jogador para lá.
        if (spawn != null && xrOrigin != null)
        {
            xrOrigin.SetPositionAndRotation(spawn.transform.position, spawn.transform.rotation);
        }

        // Agora descarrega a cena da casa.
        AsyncOperation unloadHouse = SceneManager.UnloadSceneAsync(houseSceneName);

        // Espera até a casa terminar de ser descarregada.
        if (unloadHouse != null)
        {
            while (!unloadHouse.isDone)
                yield return null;
        }

        // Esconde a tela de loading.
        HideLoading();

        // Libera novas ações.
        isTransitioning = false;
    }

    // Mostra a tela de loading.
    private void ShowLoading()
    {
        if (loadingCanvas != null)
            loadingCanvas.enabled = true;

        if (loadingGroup != null)
        {
            loadingGroup.alpha = 1f;
            loadingGroup.blocksRaycasts = true;
            loadingGroup.interactable = true;
        }
    }

    // Esconde a tela de loading.
    private void HideLoading()
    {
        if (loadingCanvas != null)
            loadingCanvas.enabled = false;

        if (loadingGroup != null)
        {
            loadingGroup.alpha = 0f;
            loadingGroup.blocksRaycasts = false;
            loadingGroup.interactable = false;
        }
    }
}