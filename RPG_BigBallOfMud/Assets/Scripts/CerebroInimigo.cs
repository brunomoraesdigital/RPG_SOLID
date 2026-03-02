using UnityEngine;
using System.Collections;

public class CerebroInimigo : MonoBehaviour
{
    [Header("--- OBJETO DE MORTE ---")]
    public GameObject prefabCadaver; // 🟤 Arraste o círculo marrom aqui!

    [Header("--- ZONAS FIXAS (Âncora no Spawn) ---")]
    public float raioPatrulha = 3f;
    public float raioPerseguicao = 6f;

    [Header("--- ZONA MÓVEL (Segue o Inimigo) ---")]
    public float raioVisao = 3f;

    [Header("--- STATUS ---")]
    public float velocidade = 2f;
    public float hpMaximo = 500f;
    public float hpAtual = 500f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private Transform alvoJogador;
    private Vector2 pontoInicial;
    private Vector2 destinoPatrulha;

    private bool estaPerseguindo = false;
    private bool estaEsperando = false;
    private bool estaMorto = false;
    private float cronometroAbortar = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        pontoInicial = transform.position;
        destinoPatrulha = pontoInicial;

        GameObject p = GameObject.Find("Jogador");
        if (p != null) alvoJogador = p.transform;
    }

    void FixedUpdate()
    {
        if (alvoJogador == null || estaMorto) return;

        float distCorpoAoJogador = Vector2.Distance(transform.position, alvoJogador.position);
        float distSpawnAoJogador = Vector2.Distance(pontoInicial, alvoJogador.position);

        // REGRA: Persegue se estiver na visão E território OU se já estiver perseguindo (ataque)
        if ((distCorpoAoJogador <= raioVisao && distSpawnAoJogador <= raioPerseguicao) || (estaPerseguindo && distSpawnAoJogador <= raioPerseguicao))
        {
            if (!estaPerseguindo)
            {
                estaPerseguindo = true;
                StopAllCoroutines();
                estaEsperando = false;
            }
        }
        else
        {
            if (estaPerseguindo)
            {
                estaPerseguindo = false;
                destinoPatrulha = pontoInicial;
            }
        }

        if (estaPerseguindo) MoverPara(alvoJogador.position);
        else ExecutarPatrulha();
    }

    public void ReceberDano(float dano)
    {
        if (estaMorto) return;

        hpAtual -= dano;
        estaPerseguindo = true;
        Debug.Log("Inimigo atingido! HP: " + hpAtual);

        if (hpAtual <= 0) Morrer();
    }

    void Morrer()
    {
        estaMorto = true;
        estaPerseguindo = false;
        rb.linearVelocity = Vector2.zero;

        // 🟤 1. Cria o cadáver no chão
        if (prefabCadaver != null)
        {
            Instantiate(prefabCadaver, transform.position, Quaternion.identity);
        }

        // 👻 2. "Esconde" o inimigo enquanto ele espera o respawn
        sr.enabled = false;
        col.enabled = false;

        Debug.Log("Inimigo derrotado! Renascendo em 30s...");
        StartCoroutine(RotinaRespawn());
    }

    IEnumerator RotinaRespawn()
    {
        yield return new WaitForSeconds(30f);

        // Só renasce se o jogador estiver longe do ponto inicial
        while (Vector2.Distance(pontoInicial, alvoJogador.position) < raioPatrulha + 2f)
        {
            yield return new WaitForSeconds(2f);
        }

        // ✨ Ressurreição: Reativa o visual e a física
        estaMorto = false;
        hpAtual = hpMaximo;
        transform.position = pontoInicial;
        sr.enabled = true;
        col.enabled = true;

        Debug.Log("Inimigo renasceu!");
    }

    void ExecutarPatrulha()
    {
        float distDestino = Vector2.Distance(transform.position, destinoPatrulha);
        cronometroAbortar += Time.fixedDeltaTime;

        if (distDestino < 0.3f || cronometroAbortar > 3f)
        {
            if (!estaEsperando) StartCoroutine(EsperarEPatroar());
        }
        else MoverPara(destinoPatrulha);
    }

    void MoverPara(Vector2 alvo)
    {
        Vector2 direcao = (alvo - (Vector2)transform.position).normalized;
        rb.linearVelocity = direcao * velocidade;
    }

    IEnumerator EsperarEPatroar()
    {
        estaEsperando = true;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(1.0f);
        destinoPatrulha = pontoInicial + (Random.insideUnitCircle * raioPatrulha);
        cronometroAbortar = 0f;
        estaEsperando = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, raioVisao);

        Vector2 centro = Application.isPlaying ? pontoInicial : (Vector2)transform.position;
        Gizmos.color = new Color(1, 0.5f, 0);
        Gizmos.DrawWireSphere(centro, raioPatrulha);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centro, raioPerseguicao);
    }
}