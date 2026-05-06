using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName) // Método público para iniciar o processo de carregamento de cena
    {
        StartCoroutine(Load(sceneName));
    }

    IEnumerator Load(string sceneName) // Coroutine para carregar a cena de forma assíncrona, permitindo mostrar a UI de carregamento durante o processo
    {
        PersistentLoadingUI.Instance.Show(); // Mostra a UI de carregamento antes de iniciar o processo de carregamento da cena

        // yield return new WaitForSeconds(0.5f); // optional delay

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName); // Inicia o carregamento assíncrono da cena, o que permite que a UI de carregamento seja exibida durante o processo

        while (!op.isDone) // Enquanto a cena não estiver completamente carregada, continua esperando
        {
            yield return null;
        }

        PersistentLoadingUI.Instance.Hide(); // Esconde a UI de carregamento após a cena ter sido completamente carregada
    }
}