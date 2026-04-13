using UnityEngine; 
using UnityEngine.InputSystem; 
// Importa o sistema novo de input da Unity (teclado, mouse, etc.)

public class FlyCamera : MonoBehaviour 
// Classe que controla uma câmera estilo "fly" (voo livre)
{
    public float speed = 5f; 
    // Velocidade de movimento da câmera

    public float mouseSensitivity = 0.1f; 
    // Sensibilidade do mouse para olhar ao redor

    float rotationX = 0f; 
    // Rotação horizontal (esquerda/direita)

    float rotationY = 0f; 
    // Rotação vertical (cima/baixo)

    void Update() 
    // Atualizado a cada frame
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        // Lê o movimento do mouse desde o último frame

        rotationX += mouseDelta.x * mouseSensitivity;
        // Aplica rotação horizontal baseada no movimento do mouse

        rotationY -= mouseDelta.y * mouseSensitivity;
        // Aplica rotação vertical (negativo para manter sensação natural)

        rotationY = Mathf.Clamp(rotationY, -90f, 90f);
        // Limita a rotação vertical para evitar virar a câmera completamente

        transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);
        // Aplica a rotação final na câmera

        Vector2 moveInput = Vector2.zero;
        // Vetor que guarda entrada de movimento (WASD)

        if (Keyboard.current.wKey.isPressed)
            moveInput.y += 1;
        // W = andar para frente

        if (Keyboard.current.sKey.isPressed)
            moveInput.y -= 1;
        // S = andar para trás

        if (Keyboard.current.dKey.isPressed)
            moveInput.x += 1;
        // D = andar para direita

        if (Keyboard.current.aKey.isPressed)
            moveInput.x -= 1;
        // A = andar para esquerda

        Vector3 move = transform.forward * moveInput.y + transform.right * moveInput.x;
        // Converte input 2D em movimento no espaço 3D relativo à câmera

        if (Keyboard.current.spaceKey.isPressed)
            move += Vector3.up;
        // Espaço = subir

        if (Keyboard.current.leftShiftKey.isPressed)
            move += Vector3.down;
        // Shift esquerdo = descer

        transform.position += move * speed * Time.deltaTime;
        // Aplica o movimento final na posição da câmera
        // Time.deltaTime garante movimento suave independente do FPS
    }
}