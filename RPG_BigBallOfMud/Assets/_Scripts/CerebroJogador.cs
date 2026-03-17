using UnityEngine;
using UnityEngine.InputSystem;

public class CerebroJogador : MonoBehaviour
{
    public float velocidadeMovimento = 5f;
    private Rigidbody2D componenteFisica;
    private Vector2 direcaoInput;

    [Header("Configurações de Ataque")]
    private float tempoExibicaoAtaque = 0.2f;

    void Start()
    {
        componenteFisica = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (GerenteConsole.instancia != null && GerenteConsole.instancia.EstaDigitando()) return;

        MoverERotacionar();

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            ExecutarAcerto();
        }
    }

    void MoverERotacionar()
    {
        if (Keyboard.current != null)
        {
            float x = 0; float y = 0;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y = 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y = -1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1;

            direcaoInput = new Vector2(x, y);

            if (direcaoInput != Vector2.zero)
            {
                float angulo = Mathf.Atan2(direcaoInput.y, direcaoInput.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angulo);
            }
        }
    }

    void FixedUpdate()
    {
        componenteFisica.linearVelocity = direcaoInput.normalized * velocidadeMovimento;
    }

    void ExecutarAcerto()
    {
        GameObject bastao = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Destroy(bastao.GetComponent<MeshCollider>());

        // CORREÇÃO: O bastão agora herda a rotação do jogador imediatamente
        bastao.transform.SetParent(this.transform);
        bastao.transform.localPosition = new Vector3(1.2f, 0, 0);
        bastao.transform.localRotation = Quaternion.identity; // Fica alinhado ao 'right' do pai
        bastao.transform.localScale = new Vector3(1.5f, 0.3f, 1f);

        bastao.GetComponent<Renderer>().material.color = new Color(0.5f, 0.8f, 1f);
        Destroy(bastao, tempoExibicaoAtaque);

        Vector2 pontoAtaque = (Vector2)transform.position + (Vector2)(transform.right * 1.5f);
        Collider2D[] atingidos = Physics2D.OverlapBoxAll(pontoAtaque, new Vector2(1.5f, 0.5f), transform.eulerAngles.z);

        foreach (var hit in atingidos)
        {
            if (hit.CompareTag("Inimigo"))
            {
                CerebroInimigo inimigo = hit.GetComponent<CerebroInimigo>();
                if (inimigo != null) inimigo.ReceberAcerto();
            }
        }
    }
}