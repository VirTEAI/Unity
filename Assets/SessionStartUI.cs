using TMPro; // Importa o TextMeshPro, que é usado nos campos de texto modernos da Unity
using UnityEngine; // Importa as funções principais da Unity
using UnityEngine.UI; // Importa os elementos de interface como Button
using UnityEngine.Networking; // Importa a biblioteca para fazer requisições HTTP
using System.Collections; // Importa a biblioteca para usar corrotinas

public class SessionStartUI : MonoBehaviour // Cria um componente que pode ser colocado em um objeto da cena
{
    public PT_MouseLook mouseLook; // Referência para o script que controla o olhar do jogador, usado para travar a câmera enquanto a UI estiver aberta

    [Header("UI")] // Só serve para organizar os campos no Inspector da Unity
    public TMP_InputField sessionInput; // Campo onde o usuário vai digitar o ID da sessão
    public Button startButton; // Botão que o usuário vai clicar para iniciar
    public GameObject panelRoot; // Objeto raiz do painel da UI, usado para esconder a tela depois

    [Header("Game")] // Organização visual no Inspector
    public GazeTracker gazeTracker; // Referência para o script que vai registrar o olhar do usuário

    public FreezeTransform freezeScript; // Referência para o script que trava a câmera enquanto a UI estiver aberta

    private void Start() // Função que roda uma vez quando o objeto começa a funcionar
    {
        freezeScript.freeze = true; // Faz a câmera ficar travada enquanto a tela de início está aberta

        mouseLook.DisableLook(); // Desativa o controle de olhar para evitar que o usuário possa olhar ao redor enquanto a UI estiver ativa

        startButton.onClick.AddListener(OnStartClicked); 
        // Diz ao botão para chamar a função OnStartClicked quando ele for clicado
    }

    private void OnDestroy() // Função chamada quando o objeto é destruído ou a cena é encerrada
    {
        startButton.onClick.RemoveListener(OnStartClicked); 
        // Remove a conexão com o botão para evitar problemas ou chamadas duplicadas
    }

    private void OnStartClicked() // Função executada quando o botão "Start" é clicado
    {
        string sessionId = sessionInput.text.Trim(); // Pega o texto digitado no campo e remove espaços no começo e no fim

        if (string.IsNullOrEmpty(sessionId)) // Verifica se o campo está vazio ou sem conteúdo válido
        {
            Debug.LogWarning("ID da sessão não pode ser vazio."); // Mostra aviso no Console
            return; // Para a função aqui, sem iniciar a sessão
        }

        StartCoroutine(ValidateSession(sessionId)); // Inicia uma corrotina para validar o ID da sessão, que é uma função que pode esperar por respostas sem travar o jogo
    }

    IEnumerator ValidateSession(string sessionId)
    {
        string url = "https://virteai-backend-tcc.onrender.com/sessions/get-id/" + sessionId; // URL do endpoint de validação de sessão

        using (UnityWebRequest request = UnityWebRequest.Get(url)) // Cria uma requisição GET para a URL
        {
            yield return request.SendWebRequest(); // Envia a requisição e aguarda a resposta

            if (request.result != UnityWebRequest.Result.Success) // Verifica se a requisição foi bem-sucedida
            {
                Debug.Log("Erro na requisição: " + request.error); // Loga o erro caso a requisição falhe
                yield break;
            }

            string responseText = request.downloadHandler.text; // Obtém o texto da resposta
            bool exists = responseText == "true"; // Verifica se a resposta indica que a sessão existe

            if (exists)
            {
                Debug.Log("Sessão válida."); // Loga que a sessão é válida
                StartSession(sessionId); // Chama a função para iniciar a sessão
            }
            else
            {
                Debug.Log("Sessão inválida."); // Loga que a sessão é inválida
            }
        }
    }

    private void StartSession(string sessionId)
    {
        gazeTracker.BeginSession(sessionId); // Envia o ID digitado para o GazeTracker, que vai começar a registrar os dados

        freezeScript.Unlock(); // Destrava a câmera para que o usuário possa se mover e olhar ao redor

        mouseLook.EnableLook(); // Reativa o controle de olhar para que o usuário possa olhar ao redor

        if (panelRoot != null) // Se o painel existir...
            panelRoot.SetActive(false); // ...esconde a interface da tela inicial
    }
}