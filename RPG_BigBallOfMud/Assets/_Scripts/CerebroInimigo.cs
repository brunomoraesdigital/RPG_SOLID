using UnityEngine;
using UnityEngine.AI;

public class CerebroInimigo : MonoBehaviour
{
    public enum Temperamento { Lobo_Agressivo, Rato_PassivoAgressivo, Galinha_PassivoCovarde, Coelho_Covarde }
    [Header("Configuracao de Tipo")]
    public Temperamento tipoMonstro;

    [SerializeField] Transform JOGADOR;
    private NavMeshAgent agent;
    private Vector3 pontoRespawn;

    [Header("Raios de Acao")]
    [SerializeField] float raioVadiagem = 3f;
    [SerializeField] float raioVisao = 4f;
    [SerializeField] float raioPerseguicao = 8f;
    [SerializeField] float alcanceAtaque = 1.6f;

    private bool estaBravo = false;
    private bool estaComMedo = false;
    private bool mensagemAcaoDisparada = false;

    private float cronometroPatrulha;
    private float cronometroAtaque;
    [SerializeField] float tempoEsperaPatrulha = 2f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        pontoRespawn = transform.position;
        cronometroPatrulha = tempoEsperaPatrulha;

        if (GetComponent<Rigidbody2D>() != null)
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        if (JOGADOR == null) return;

        float distJogador = Vector3.Distance(transform.position, JOGADOR.position);
        float distJogadorProSpawn = Vector3.Distance(pontoRespawn, JOGADOR.position);
        float distInimigoProSpawn = Vector3.Distance(transform.position, pontoRespawn);

        bool deveFugir = (tipoMonstro == Temperamento.Coelho_Covarde && distJogador <= raioVisao) ||
                         (estaComMedo && distJogador <= raioVisao);

        if (deveFugir)
        {
            if (!mensagemAcaoDisparada && tipoMonstro == Temperamento.Coelho_Covarde)
            {
                EnviarLog("O coelho iniciou a fuga desesperada pulando o mais rapido que pode!");
                mensagemAcaoDisparada = true;
            }
            FugirDoJogador();
        }
        else if ((tipoMonstro == Temperamento.Lobo_Agressivo && distJogador <= raioVisao && distJogadorProSpawn <= raioPerseguicao) ||
                 (estaBravo && distJogadorProSpawn <= raioPerseguicao))
        {
            if (!mensagemAcaoDisparada && tipoMonstro == Temperamento.Lobo_Agressivo)
            {
                EnviarLog("O lobo iniciou a perseguicao com as suas presas a mostra!");
                mensagemAcaoDisparada = true;
            }

            OlharParaAlvo(JOGADOR.position);

            if (distJogador <= alcanceAtaque)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                ExecutarAtaqueMonstro();
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(JOGADOR.position);
            }
        }
        else
        {
            if (mensagemAcaoDisparada)
            {
                // RESET PARA QUEM FOGE (Galinha/Coelho)
                if ((estaComMedo || tipoMonstro == Temperamento.Coelho_Covarde) && distJogador > raioVisao)
                {
                    if (tipoMonstro == Temperamento.Galinha_PassivoCovarde && estaComMedo) EnviarLog("A galinha saiu da sua visao, ela esta voltando pro seu ninho.");
                    if (tipoMonstro == Temperamento.Coelho_Covarde) EnviarLog("O coelho conseguiu sair da sua visao, ele esta voltando pra sua toca.");

                    mensagemAcaoDisparada = false;
                    estaComMedo = false; // Galinha volta a ser passiva aqui
                }
                // RESET PARA QUEM PERSEGUE (Lobo/Rato)
                else if (distJogadorProSpawn > raioPerseguicao || (tipoMonstro == Temperamento.Lobo_Agressivo && distJogador > raioVisao))
                {
                    if (tipoMonstro == Temperamento.Lobo_Agressivo) EnviarLog("O lobo desistiu da perseguicao com ar de satisfacao.");
                    if (tipoMonstro == Temperamento.Rato_PassivoAgressivo) EnviarLog("O rato desistiu da perseguicao com ar indignado.");

                    mensagemAcaoDisparada = false;
                    estaBravo = false; // Rato para de te seguir aqui
                }
            }

            Patrulhar();
        }

        if (!agent.isStopped) AjustarRotacaoVisual();
    }

    void EnviarLog(string texto)
    {
        if (GerenteConsole.instancia != null)
        {
            GerenteConsole.instancia.EscreverNoConsole(texto);
        }
    }

    void OlharParaAlvo(Vector3 alvo)
    {
        Vector3 direcao = (alvo - transform.position).normalized;
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo);
    }

    void ExecutarAtaqueMonstro()
    {
        cronometroAtaque += Time.deltaTime;
        if (cronometroAtaque >= 1f)
        {
            string nomeInimigo = tipoMonstro.ToString().Split('_')[0];
            EnviarLog("O " + nomeInimigo.ToLower() + " ataca com ferocidade!");

            GameObject bastao = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(bastao.GetComponent<MeshCollider>());
            bastao.transform.SetParent(this.transform);
            bastao.transform.localPosition = new Vector3(0.8f, 0, -0.1f);
            bastao.transform.localRotation = Quaternion.identity;
            bastao.transform.localScale = new Vector3(1f, 0.2f, 1f);
            bastao.GetComponent<Renderer>().material.color = Color.red;
            bastao.GetComponent<Renderer>().sortingOrder = 10;

            Destroy(bastao, 0.2f);
            cronometroAtaque = 0;
        }
    }

    public void ReceberAcerto()
    {
        string nomeInimigo = tipoMonstro.ToString().Split('_')[0];
        EnviarLog("O jogador acertou o " + nomeInimigo.ToLower() + "!");

        if (tipoMonstro == Temperamento.Rato_PassivoAgressivo && !estaBravo)
        {
            estaBravo = true;
            mensagemAcaoDisparada = true;
            EnviarLog("O rato iniciou a perseguicao com furia nos olhos!");
        }
        if (tipoMonstro == Temperamento.Galinha_PassivoCovarde && !estaComMedo)
        {
            estaComMedo = true;
            mensagemAcaoDisparada = true;
            EnviarLog("A galinha iniciou a fuga desesperada batendo as asas o mais rapido que pode!");
        }
    }

    void FugirDoJogador()
    {
        agent.isStopped = false;
        Vector3 direcaoOposta = transform.position - JOGADOR.position;
        Vector3 destinoFuga = transform.position + direcaoOposta.normalized * 5f;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(destinoFuga, out hit, 5f, NavMesh.AllAreas)) agent.SetDestination(hit.position);
    }

    void Patrulhar()
    {
        agent.isStopped = false;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            cronometroPatrulha -= Time.deltaTime;
            if (cronometroPatrulha <= 0)
            {
                Vector2 p = Random.insideUnitCircle * raioVadiagem;
                Vector3 d = pontoRespawn + new Vector3(p.x, p.y, 0);
                NavMeshHit h;
                if (NavMesh.SamplePosition(d, out h, 1.0f, NavMesh.AllAreas)) agent.SetDestination(h.position);
                cronometroPatrulha = tempoEsperaPatrulha;
            }
        }
    }

    void AjustarRotacaoVisual()
    {
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            float angulo = Mathf.Atan2(agent.velocity.y, agent.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angulo);
        }
    }
}