using UnityEngine;
// Precisamos desta linha abaixo para o novo sistema de input funcionar
using UnityEngine.InputSystem;

public class CerebroJogador : MonoBehaviour
{
    // O segredo é o ": MonoBehaviour" ali em cima. 
    // Sem isso, o Unity não aceita o script no objeto.public float velocidade = 5f;

    public float velocidadeMovimento = 5f;
    private Rigidbody2D componenteFisica;
    private Vector2 direcaoInput;

    void Start()
    {
        componenteFisica = GetComponent<Rigidbody2D>();

    }

    void Update()
    {
        if (GerenteConsole.instancia != null && GerenteConsole.instancia.EstaDigitando()) return;
        if (Keyboard.current != null)
        {
            float x = 0;
            float y = 0;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y = 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y = -1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1;

            direcaoInput = new Vector2(x, y);

            // LOGICA DE ROTAÇÃO: Se houver algum movimento, vira o personagem
            if (direcaoInput != Vector2.zero)
            {
                // Calcula o ângulo baseado no movimento
                float angulo = Mathf.Atan2(direcaoInput.y, direcaoInput.x) * Mathf.Rad2Deg;
                // Aplica a rotação no Transform
                transform.rotation = Quaternion.Euler(0, 0, angulo);
            }
        }
    }

    void FixedUpdate()
    {
        componenteFisica.MovePosition(componenteFisica.position + direcaoInput.normalized * velocidadeMovimento * Time.fixedDeltaTime);
    }
}
