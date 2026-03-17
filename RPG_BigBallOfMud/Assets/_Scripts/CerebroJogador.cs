using UnityEngine;
using UnityEngine.InputSystem;

public class CerebroJogador : MonoBehaviour
{
    public float velocidadeMovimento = 5f;
    private Rigidbody2D componenteFisica;
    private Vector2 direcaoInput;

    [Header("Configurações de Ataque")]
    private float tempoExibicaoAtaque = 0.2f;

    private Transform alvoPerseguicao;
    private Vector3? pontoDestinoMouse = null; // Destino no chão
    private float cronometroAtaqueAuto;

    void Start()
    {
        componenteFisica = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (GerenteConsole.instancia != null && GerenteConsole.instancia.EstaDigitando()) return;

        MoverERotacionar();

        if (Keyboard.current.fKey.wasPressedThisFrame) { ExecutarAcerto(); }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            DetectarCliqueMouse();
        }

        if (alvoPerseguicao != null) { ExecutarAutoCaca(); }
        else if (pontoDestinoMouse != null) { IrParaPontoMouse(); }
    }

    void DetectarCliqueMouse()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Inimigo"))
        {
            alvoPerseguicao = hit.collider.transform;
            pontoDestinoMouse = null;
        }
        else
        {
            alvoPerseguicao = null;
            pontoDestinoMouse = new Vector3(mousePos.x, mousePos.y, 0);
        }
    }

    void IrParaPontoMouse()
    {
        float distancia = Vector2.Distance(transform.position, pontoDestinoMouse.Value);
        if (distancia > 0.2f)
        {
            Vector2 direcao = ((Vector2)pontoDestinoMouse.Value - (Vector2)transform.position).normalized;
            direcaoInput = direcao;
            float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angulo);
        }
        else
        {
            pontoDestinoMouse = null;
            direcaoInput = Vector2.zero;
        }
    }

    void ExecutarAutoCaca()
    {
        float distancia = Vector2.Distance(transform.position, alvoPerseguicao.position);
        if (distancia > 1.2f)
        {
            Vector2 direcao = (alvoPerseguicao.position - transform.position).normalized;
            direcaoInput = direcao;
            float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angulo);
        }
        else
        {
            direcaoInput = Vector2.zero;
            cronometroAtaqueAuto += Time.deltaTime;
            if (cronometroAtaqueAuto >= 1f)
            {
                ExecutarAcerto();
                cronometroAtaqueAuto = 0;
            }
        }
    }

    void MoverERotacionar()
    {
        float x = 0; float y = 0;
        bool temTeclado = false;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) { y = 1; temTeclado = true; }
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) { y = -1; temTeclado = true; }
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) { x = -1; temTeclado = true; }
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) { x = 1; temTeclado = true; }

        if (temTeclado)
        {
            alvoPerseguicao = null;
            pontoDestinoMouse = null;
            direcaoInput = new Vector2(x, y);
            float angulo = Mathf.Atan2(direcaoInput.y, direcaoInput.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angulo);
        }
        else if (alvoPerseguicao == null && pontoDestinoMouse == null)
        {
            direcaoInput = Vector2.zero;
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
        bastao.transform.SetParent(this.transform);
        bastao.transform.localPosition = new Vector3(1.2f, 0, 0);
        bastao.transform.localRotation = Quaternion.identity;
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