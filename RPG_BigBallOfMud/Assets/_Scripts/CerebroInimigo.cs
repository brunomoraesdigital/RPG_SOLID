using UnityEngine;
using UnityEngine.AI;

public class CerebroInimigo : MonoBehaviour
{
    [Header("Configuracao de Tipo")]
    public Temperamento tipoMonstro;

    [Header("Raios de Acao")]
    [SerializeField] float raioVadiagem = 3f;
    [SerializeField] float raioVisao = 4f;
    [SerializeField] float raioPerseguicao = 8f;
    [SerializeField] float alcanceAtaque = 1.6f;
    [SerializeField] float tempoEsperaPatrulha = 2f;

    [SerializeField] Transform JOGADOR;

    private NavMeshAgent agent;
    private Vector3 pontoRespawn;

    private IMovimento movimento;
    private ILogger logger;
    private IPercepcao percepcao;
    private IAtaque ataque;
    private EstadoInimigo estadoInimigo;
    private DecisorIA decisorIA;
    private ConfiguracaoInimigo configuracao;

    // Propriedade pública para acesso pelos comportamentos
    public Vector3 PontoRespawn => pontoRespawn;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        movimento = new NavMeshMovement(agent, transform);
        logger = new UnityConsoleLogger();

        estadoInimigo = new EstadoInimigo();

        configuracao = new ConfiguracaoInimigo
        {
            raioVadiagem = raioVadiagem,
            raioVisao = raioVisao,
            raioPerseguicao = raioPerseguicao,
            alcanceAtaque = alcanceAtaque,
            tempoEsperaPatrulha = tempoEsperaPatrulha
        };
        pontoRespawn = transform.position;
        percepcao = new Percepcao(transform, JOGADOR, pontoRespawn, configuracao);

        ataque = new AtaqueBasico();

        decisorIA = new DecisorIA(percepcao, estadoInimigo, configuracao, logger, tipoMonstro);

        if (GetComponent<Rigidbody2D>() != null)
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        if (JOGADOR == null) return;

        ataque.AtualizarCooldown();

        IComportamentoIA comportamento = decisorIA.Decidir();
        comportamento.Executar(this, movimento, ataque, logger, estadoInimigo, tipoMonstro, percepcao, configuracao);

        if (!movimento.EstaParado)
            movimento.RotacionarParaVelocidade();
    }

    public void ReceberAcerto()
    {
        string nomeInimigo = tipoMonstro.ToString().Split('_')[0];
        logger.Log($"O jogador acertou o {nomeInimigo.ToLower()}!");

        if (tipoMonstro == Temperamento.Rato_PassivoAgressivo)
        {
            if (!estadoInimigo.EstaBravo)
            {
                estadoInimigo.AtivarFuria();
                logger.Log("O rato iniciou a perseguicao com furia nos olhos!");
            }
        }
        else if (tipoMonstro == Temperamento.Galinha_PassivoCovarde)
        {
            if (!estadoInimigo.EstaComMedo)
            {
                estadoInimigo.AtivarMedo();
                logger.Log("A galinha iniciou a fuga desesperada batendo as asas o mais rapido que pode!");
            }
        }
    }

    public Transform GetTransform() => transform;
}

public enum Temperamento { Lobo_Agressivo, Rato_PassivoAgressivo, Galinha_PassivoCovarde, Coelho_Covarde }