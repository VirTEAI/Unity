using System; // Dá acesso a tipos como DateTime e atributos como [Serializable]
using System.Collections; // Permite usar IEnumerator para corrotinas
using System.Collections.Generic; // Permite usar List<T>
using System.Text; // Permite usar StringBuilder para construir strings de forma eficiente
// using System.IO; // Permite criar e salvar arquivos no disco
using UnityEngine; // Importa as funções principais da Unity
using UnityEngine.Networking; // Permite fazer requisições web, como validar o ID da sessão






using TMPro;





public class GazeTracker : MonoBehaviour // Cria um componente que vai controlar a leitura do olhar
{
    [Header("References")]
    public Camera gazeCamera; // Câmera usada para simular de onde o usuário está olhando

    [Header("Raycast Settings")]
    public float maxDistance = 20f; // Distância máxima do raio de visão
    public LayerMask focusLayer = ~0; // Define quais layers podem ser detectadas pelo raycast
    public float sampleInterval = 0.05f; // Intervalo entre cada leitura do olhar (20 vezes por segundo)

    [Header("Backend")]
    public string postUrl = "https://virteai-backend-tcc.onrender.com/sessions/attach"; // URL para validar o ID da sessão antes de começar

    private FocusPoints currentPoint; // Guarda o ponto que está sendo olhado no momento
    private float currentEnterTime; // Guarda o tempo em que o olhar entrou no ponto atual
    private float nextSampleTime; // Guarda o próximo momento em que a leitura será feita
    private string sessionId; // ID da sessão atual, vindo do menu de entrada

    // private string jsonPath; // Caminho do arquivo JSON que será salvo
    private bool sessionStarted; // Diz se a sessão já começou ou não

    private SessionData sessionData; // Objeto principal que vai guardar todos os eventos da sessão

    [Serializable] // Diz para a Unity que essa classe pode ser convertida para JSON
    public class SessionData
    {
        public string sessionId; // ID da sessão
        public string startedAtUtc; // Data e hora em que a sessão começou, em UTC
        public List<GazeEventData> events = new List<GazeEventData>(); // Lista com todos os eventos registrados
    }

    [Serializable] // Também precisa ser serializável para virar JSON
    public class GazeEventData
    {
        public string timestampUtc; // Momento exato do evento
        public string pointId; // ID do ponto de foco que foi olhado
        public string category; // Categoria do ponto de foco
        public string eventType; // Tipo do evento: session_start, enter, exit, session_end
        public float durationSeconds; // Tempo em segundos que o usuário ficou olhando
    }

    private void Start()
    {
        if (gazeCamera == null) // Se nenhuma câmera foi atribuída no Inspector...
            gazeCamera = Camera.main; // ...usa a câmera principal da cena
    }

    public void BeginSession(string newSessionId)
    {
        sessionId = newSessionId; // Guarda o ID da sessão informado na interface
        // jsonPath = Path.Combine(Application.persistentDataPath, $"gaze_{sessionId}.json"); // Monta o caminho do arquivo JSON

        sessionData = new SessionData // Cria o objeto que vai guardar tudo da sessão
        {
            sessionId = sessionId, // Salva o ID da sessão
            startedAtUtc = DateTime.UtcNow.ToString("o") // Salva a data/hora de início em formato completo
        };

        sessionStarted = true; // Marca que a sessão já começou
        AddEvent("session_start", null, 0f); // Registra o início da sessão como um evento

        Debug.Log("Session started: " + sessionId); // Mostra no Console que a sessão começou
        // Debug.Log("JSON PATH: " + jsonPath); // Mostra onde o arquivo será salvo
    }

    private void Update()
    {
        if (!sessionStarted) // Se a sessão ainda não começou...
            return; // ...não faz nada

        if (Time.time < nextSampleTime) // Se ainda não chegou a hora da próxima leitura...
            return; // ...sai da função

        nextSampleTime = Time.time + sampleInterval; // Define quando será a próxima leitura
        SampleGaze(); // Faz a leitura do que está sendo olhado







        UpdateGazeDebugUI(); // Atualiza o UI de debug para mostrar o ponto atual e o tempo olhando para ele
    }

    private void SampleGaze()
    {
        Ray ray = new Ray(gazeCamera.transform.position, gazeCamera.transform.forward); // Cria um raio saindo da câmera para frente

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, focusLayer, QueryTriggerInteraction.Ignore))
        {
            // Se o raio acertar algum objeto dentro das regras definidas...

            FocusPoints hitPoint = hit.collider.GetComponentInParent<FocusPoints>(); // Procura o FocusPoints no objeto atingido ou no pai dele

            if (hitPoint == null)
            {
                // Se o objeto atingido não tiver FocusPoints...
                EndCurrentPoint(); // Finaliza o ponto atual, se existir
                return; // Sai da função
            }

            if (hitPoint != currentPoint) // Se o usuário começou a olhar para um ponto diferente...
            {
                EndCurrentPoint(); // Finaliza o ponto anterior
                StartNewPoint(hitPoint); // Inicia o novo ponto
            }
        }
        else
        {
            // Se o raio não atingiu nenhum objeto...
            EndCurrentPoint(); // Finaliza o ponto atual, se existir
        }
    }








    // Está sessão toda é para mostrar no UI de debug qual ponto está sendo olhado e há quanto tempo, para ajudar no desenvolvimento
    [Header("Debug UI")]
    [SerializeField] private GameObject gazeDebugPanel;
    [SerializeField] private TMP_Text gazeDebugText;

    private void UpdateGazeDebugUI()
    {
        if (gazeDebugPanel == null || gazeDebugText == null)
            return;

        if (currentPoint == null)
        {
            gazeDebugPanel.SetActive(false);
            return;
        }

        float duration = Time.unscaledTime - currentEnterTime;

        gazeDebugPanel.SetActive(true);
        gazeDebugText.text =
            $"Looking at: {currentPoint.pointId}\n" +
            $"Category: {currentPoint.category}\n" +
            $"Time: {duration:0.00}s";
    }








    private void StartNewPoint(FocusPoints point)
    {
        if (point == null) return; // Se não tiver ponto, não faz nada

        currentPoint = point; // Guarda qual ponto passou a ser olhado
        currentEnterTime = Time.unscaledTime; // Marca o tempo de entrada nesse ponto
        AddEvent("enter", point, 0f); // Registra o evento de entrada







        // Ativa o painel de debug e mostra as informações do ponto atual
        if (gazeDebugPanel != null)
        gazeDebugPanel.SetActive(true);

        UpdateGazeDebugUI();
    }

    private void EndCurrentPoint()
    {
        if (currentPoint == null) return; // Se não estiver olhando para nada, não faz nada

        float durationSeconds = (Time.unscaledTime - currentEnterTime); // Calcula quanto tempo o usuário ficou olhando para o ponto atual (em segundos)
        AddEvent("exit", currentPoint, durationSeconds); // Registra o evento de saída com a duração
        currentPoint = null; // Limpa o ponto atual









        if (gazeDebugPanel != null)
            gazeDebugPanel.SetActive(false); // Esconde o painel de debug quando não estiver olhando para nenhum ponto
    }

    private void AddEvent(string eventType, FocusPoints point, float durationSeconds)
    {
        if (sessionData == null) // Se a sessão ainda não foi criada...
            return; // ...não salva nada

        sessionData.events.Add(new GazeEventData // Adiciona um novo evento na lista
        {
            timestampUtc = DateTime.UtcNow.ToString("o"), // Hora atual em UTC
            pointId = point != null ? point.pointId : "", // Se existir ponto, salva o ID; senão, deixa vazio
            category = point != null ? point.category : "", // Se existir ponto, salva a categoria; senão, vazio
            eventType = eventType, // Salva o tipo do evento
            durationSeconds = durationSeconds // Salva a duração
        });
    }

    private IEnumerator SendSessionToBackend()
    {
        if (sessionData == null) yield break; // Se não tiver dados da sessão, não faz nada

        string json = JsonUtility.ToJson(sessionData, true); // Converte os dados para JSON bonito/formatado
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json); // Converte o JSON para bytes para enviar na requisição

        using (UnityWebRequest request = new UnityWebRequest(postUrl, "POST")) // Cria uma requisição POST para a URL definida
        {
            request.timeout = 10; // Define um tempo limite para a requisição (em segundos)
            request.uploadHandler = new UploadHandlerRaw(bodyRaw); // Define o corpo da requisição com os dados JSON
            request.downloadHandler = new DownloadHandlerBuffer(); // Define um handler para receber a resposta
            request.SetRequestHeader("Content-Type", "application/json"); // Define o header para indicar que o corpo é JSON

            yield return request.SendWebRequest(); // Envia a requisição e espera pela resposta








            // Mostra na Ui de Debug a resposta do servidor, para ajudar no desenvolvimento e ver se deu certo
            gazeDebugPanel.SetActive(true);
            gazeDebugText.text = "Mandando dados para o servidor...\n";








            if (request.result != UnityWebRequest.Result.Success) // Se a requisição falhou...
            {
                // Debug.LogError("Failed to send session data: " + request.error); // Mostra o erro no Console
                // Debug.LogError("Response: " + request.downloadHandler.text); // Mostra a resposta do servidor, que pode conter detalhes do erro







                gazeDebugText.text += "Erro ao enviar dados:\n" + request.error + "\nResposta: " + request.downloadHandler.text; // Mostra o erro e a resposta do servidor na UI de debug
                yield break; // Sai da função
            }

            // Debug.Log("Session data sent successfully! Response: " + request.downloadHandler.text); // Mostra a resposta do servidor









            gazeDebugText.text += "Dados enviados com sucesso!\nResposta: " + request.downloadHandler.text; // Mostra a resposta do servidor na UI de debug
        }
    }

    // private void SaveJson()
    // {
    //     if (sessionData == null) // Se não houver dados da sessão...
    //         return; // ...não faz nada

    //     string json = JsonUtility.ToJson(sessionData, true); // Converte os dados para JSON bonito/formatado
    //     File.WriteAllText(jsonPath, json); // Escreve o JSON no arquivo
    //     Debug.Log("Saved JSON to: " + jsonPath); // Mostra no Console onde salvou
    // }

    private IEnumerator FinishSessionAndSend()
    {
        if (!sessionStarted) yield break; // Se a sessão nem chegou a começar, não faz nada

        EndCurrentPoint(); // Finaliza qualquer ponto que ainda esteja em andamento
        AddEvent("session_end", null, 0f); // Registra o fim da sessão
        sessionStarted = false; // Marca que a sessão terminou para evitar novas leituras ou envios

        yield return StartCoroutine(SendSessionToBackend()); // Envia os dados para o backend

        // SaveJson(); // Salva o arquivo JSON no disco
    }

    public void EndSessionAndExit() // Função pública para ser chamada pelo menu de saída
    {
        StartCoroutine(EndSessionAndExitRoutine());
    }

    private IEnumerator EndSessionAndExitRoutine() // Corrotina para finalizar a sessão, enviar os dados e depois fechar o aplicativo
    {
        yield return StartCoroutine(FinishSessionAndSend()); // Finaliza a sessão e inicia o envio dos dados

        // when finishsessionandsend finished, turn the object red
        GetComponent<Renderer>().material.color = Color.red;

        // yield return new WaitForSeconds(1f); // Espera um pouco para garantir que a requisição seja enviada antes de fechar o aplicativo (ajuste esse tempo se necessário)

    #if UNITY_ANDROID && !UNITY_EDITOR // Se for Android e não estiver no Editor, em vez de fechar o aplicativo, manda ele para segundo plano para evitar problemas com o envio dos dados
        // using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        // using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        {
            // activity.Call<bool>("moveTaskToBack", true);
            // Application.runInBackground = true; // Permite que o aplicativo continue rodando em segundo plano para garantir que a requisição seja enviada
            Application.Quit(); // Tenta fechar o aplicativo, mas no Android isso não funciona, então ele vai para segundo plano
        }
    #else
        Application.Quit(); // Para outras plataformas, fecha o aplicativo normalmente
    #endif // Application.Quit() não funciona no Editor, então para testar no Editor, você pode usar UnityEditor.EditorApplication.isPlaying = false; para parar a execução, mas lembre-se de que isso só deve ser usado para testes e não deve estar presente na versão final do jogo.
    }

    public void ForceSave()
    {
        StartCoroutine(FinishSessionAndSend()); // Função pública para forçar o envio dos dados, pode ser chamada por outros scripts se necessário
    }
}