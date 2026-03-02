using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CerebroJogador : MonoBehaviour
{
    public Transform indicadorDirecao;
    public float distanciaSetinha = 0.55f; // Distância da setinha à frente do jogador
    public enum TipoArma { Espada, Arco, Cajado }

    [Header("--- EQUIPAMENTO ---")]
    public TipoArma armaEquipada = TipoArma.Espada;
    public int atk_arma = 10;

    [Header("--- PROGRESSO E PONTOS ---")]
    [Range(1, 100)] public int NV = 1;
    public int pontosDisponiveis;

    [Header("--- ATRIBUTOS PRIMÁRIOS ---")]
    [Range(1, 100)] public int FOR = 1; [Range(1, 100)] public int AGI = 1;
    [Range(1, 100)] public int VIT = 1; [Range(1, 100)] public int INT = 1;
    [Range(1, 100)] public int DES = 1; [Range(1, 100)] public int SOR = 1;

    private int fOld, aOld, vOld, iOld, dOld, sOld, nvOld;

    [Header("--- STATUS SECUNDÁRIOS ---")]
    public int vida_max; public int mana_max;
    public int atk_fis, atk_mag, atk_dist, defesa;
    public int vel_ataque, esquiva, precisao, acerto_critico, esquiva_perfeita, carga_max;

    private Rigidbody2D rb;
    private Vector2 entrada;
    private float tempoReuso = 0;
    public float velocidadeBase = 5f;

    private Vector2 direcaoOlhar = Vector2.down;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SalvarEstadoAnterior();
        CalcularStatus();
    }

    void Update()
    {
        CalcularStatus();
        GerenciarInputs();
        AtualizarSetinha();
        Debug.DrawRay(transform.position, direcaoOlhar * ((armaEquipada == TipoArma.Espada) ? 1.5f : 6f), Color.white);
    }

    void AtualizarSetinha()
    {
        if (indicadorDirecao != null)
        {
            // 🎯 OBRBITAÇÃO PRÓXIMA: Multiplica a direção direto pela distância curta
            Vector2 posicaoOrbita = (Vector2)transform.position + (direcaoOlhar * distanciaSetinha);
            indicadorDirecao.position = posicaoOrbita;

            // Mantém a rotação correta
            float angulo = Mathf.Atan2(direcaoOlhar.y, direcaoOlhar.x) * Mathf.Rad2Deg;
            indicadorDirecao.rotation = Quaternion.Euler(0, 0, angulo - 90);
        }
    }

    void OnValidate() { CalcularStatus(); }

    void CalcularStatus()
    {
        int totalGanhos = NV - 1;
        int gastos = (FOR - 1) + (AGI - 1) + (VIT - 1) + (INT - 1) + (DES - 1) + (SOR - 1);
        if (gastos > totalGanhos)
        {
            FOR = fOld; AGI = aOld; VIT = vOld; INT = iOld; DES = dOld; SOR = sOld; NV = nvOld;
        }
        else { SalvarEstadoAnterior(); }
        pontosDisponiveis = totalGanhos - gastos;

        atk_mag = (int)((INT * 4) + (DES / 2f) + SOR) + atk_arma;
        atk_fis = (int)((FOR * 2) + (DES / 2f) + SOR) + atk_arma;
        atk_dist = (int)(DES + (AGI / 2f) + SOR) + atk_arma;
        vida_max = (int)((VIT * 3 + NV * 2) * 100);
        mana_max = (int)((INT * 3 + NV * 2) * 100);
    }

    void SalvarEstadoAnterior()
    {
        fOld = FOR; aOld = AGI; vOld = VIT; iOld = INT; dOld = DES; sOld = SOR; nvOld = NV;
    }

    void GerenciarInputs()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado == null) return;

        float x = (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) ? -1 : (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) ? 1 : 0;
        float y = (teclado.wKey.isPressed || teclado.upArrowKey.isPressed) ? 1 : (teclado.sKey.isPressed || teclado.downArrowKey.isPressed) ? -1 : 0;
        entrada = new Vector2(x, y);

        if (entrada.magnitude > 0.1f) direcaoOlhar = entrada.normalized;

        if (tempoReuso > 0) tempoReuso -= Time.deltaTime;
        if (tempoReuso <= 0)
        {
            if (teclado.jKey.wasPressedThisFrame) ProcessarAtaque(1);
            if (teclado.kKey.wasPressedThisFrame) ProcessarAtaque(2);
            if (teclado.lKey.wasPressedThisFrame) ProcessarAtaque(3);
        }
    }

    void ProcessarAtaque(int b)
    {
        // 📏 DEFINIÇÃO DE ALCANCE (Conforme seu pedido)
        float alcance = 1.5f; // Padrão Espada
        if (armaEquipada == TipoArma.Arco) alcance = 4.5f;
        if (armaEquipada == TipoArma.Cajado) alcance = 5.5f;

        int dano = (armaEquipada == TipoArma.Espada) ? atk_fis : (armaEquipada == TipoArma.Arco ? atk_dist : atk_mag);

        // Bônus do Ataque Nível 2
        if (b == 2) dano = (int)(dano * 1.5f);

        // 🎯 ATAQUE NÍVEL 1 E 2 (LINHA RETA)
        if (b == 1 || b == 2)
        {
            RaycastHit2D[] alvos = Physics2D.RaycastAll(transform.position, direcaoOlhar, alcance);
            foreach (var hit in alvos)
            {
                // Ignora o próprio jogador e foca na Tag Inimigo
                if (hit.collider.gameObject != gameObject && hit.collider.CompareTag("Inimigo"))
                {
                    hit.collider.GetComponent<CerebroInimigo>().ReceberDano(dano);
                    Debug.Log($"Acerto Nv{b} com {armaEquipada}!");
                    break;
                }
            }
            tempoReuso = (b == 2) ? 1f : 0.3f;
        }
        // 🌀 ATAQUE NÍVEL 3 (ESPECIAIS)
        else if (b == 3)
        {
            ExecutarEspecial(alcance); // Passa o alcance para o especial saber onde buscar
            tempoReuso = 3f;
        }
    }

    void ExecutarEspecial(float alcanceMaximo)
    {
        if (armaEquipada == TipoArma.Espada)
        {
            // Espada Nv3: Impacto em área (Raio 5 ao redor do jogador)
            AplicarDanoEmArea(transform.position, 5f, atk_fis * 2);
        }
        else
        {
            // Busca o inimigo dentro do ALCANCE da arma (4.5m para arco, 5.5m para cajado)
            Vector2 pontoBusca = (Vector2)transform.position + (direcaoOlhar * (alcanceMaximo / 2f));
            Collider2D[] detectados = Physics2D.OverlapCircleAll(pontoBusca, alcanceMaximo / 2f);

            Transform alvoEspecial = null;
            foreach (var c in detectados)
            {
                if (c.CompareTag("Inimigo")) { alvoEspecial = c.transform; break; }
            }

            if (alvoEspecial != null)
            {
                if (armaEquipada == TipoArma.Arco)
                {
                    // Chuva de Flechas: Raio 5 ao redor do alvo
                    AplicarDanoEmArea(alvoEspecial.position, 5f, atk_dist * 2);
                }
                else if (armaEquipada == TipoArma.Cajado)
                {
                    // Meteoros: Espalham em raio 6, explosão de cada um raio 4
                    StartCoroutine(RotinaMeteoros(alvoEspecial.position));
                }
            }
        }
    }

    void AplicarDanoEmArea(Vector2 centro, float raio, int dano)
    {
        Collider2D[] atingidos = Physics2D.OverlapCircleAll(centro, raio);
        foreach (var c in atingidos)
        {
            if (c.CompareTag("Inimigo")) c.GetComponent<CerebroInimigo>().ReceberDano(dano);
        }
    }

    IEnumerator RotinaMeteoros(Vector2 centroAlvo)
    {
        int qtd = Random.Range(3, 6);
        for (int i = 0; i < qtd; i++)
        {
            // Meteoros caem em um raio de 6 ao redor do alvo
            Vector2 posMeteoro = centroAlvo + (Random.insideUnitCircle * 6f);
            // Cada meteoro acerta um raio de 4
            AplicarDanoEmArea(posMeteoro, 4f, atk_mag * 3);
            Debug.Log($"☄️ Meteoro {i + 1} explodiu!");
            yield return new WaitForSeconds(0.25f);
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + entrada.normalized * velocidadeBase * Time.fixedDeltaTime);
    }
}