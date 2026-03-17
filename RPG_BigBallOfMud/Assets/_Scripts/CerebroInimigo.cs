using UnityEngine;
using UnityEngine.AI;

public class CerebroInimigo : MonoBehaviour
{
    public enum Temperamento { Lobo_Agressivo, Rato_PassivoAgressivo, Galinha_PassivoCovarde, Coelho_Covarde }
    [Header("Configuração de Tipo")]
    public Temperamento tipoMonstro;

    [SerializeField] Transform JOGADOR;
    private NavMeshAgent agent;
    private Vector3 pontoRespawn;

    [Header("Raios de Ação")]
    [SerializeField] float raioVadiagem = 3f;      // Amarelo
    [SerializeField] float raioVisao = 4f;         // Azul
    [SerializeField] float raioPerseguicao = 8f;   // Vermelho

    private bool estaBravo = false;
    private bool estaComMedo = false;
    private float cronometroPatrulha;
    [SerializeField] float tempoEsperaPatrulha = 2f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        pontoRespawn = transform.position;
        cronometroPatrulha = tempoEsperaPatrulha;

        if (GetComponent<Rigidbody2D>() != null)
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
    }

    void Update()
    {
        if (JOGADOR == null) return;

        float distJogador = Vector3.Distance(transform.position, JOGADOR.position);
        float distJogadorProSpawn = Vector3.Distance(pontoRespawn, JOGADOR.position);
        float distInimigoProSpawn = Vector3.Distance(transform.position, pontoRespawn);

        // 1. LÓGICA DE FUGA (COELHO OU GALINHA QUE APANHOU)
        // A Galinha continua com medo enquanto o jogador estiver na visão OU enquanto não chegar no spawn
        bool deveFugir = (tipoMonstro == Temperamento.Coelho_Covarde && distJogador <= raioVisao) ||
                         (estaComMedo && distJogador <= raioVisao);

        if (deveFugir)
        {
            FugirDoJogador();
        }
        // 2. LÓGICA DE PERSEGUIÇÃO (LOBO OU RATO QUE APANHOU)
        else if ((tipoMonstro == Temperamento.Lobo_Agressivo && distJogador <= raioVisao && distJogadorProSpawn <= raioPerseguicao) ||
                 (estaBravo && distJogadorProSpawn <= raioPerseguicao))
        {
            agent.isStopped = false;
            agent.SetDestination(JOGADOR.position);
        }
        // 3. RETORNO E VADIAGEM
        else
        {
            // REGRA DA GALINHA: Só volta a ser passiva quando estiver perto do spawn e longe do jogador
            if (estaComMedo && distInimigoProSpawn <= 1.0f && distJogador > raioVisao)
            {
                estaComMedo = false;
            }

            // REGRA DO RATO: Volta a ser passivo se o jogador sair da área de perseguição
            if (estaBravo && distJogadorProSpawn > raioPerseguicao)
            {
                estaBravo = false;
            }

            Patrulhar();
        }

        AjustarRotacaoVisual();
    }

    public void ReceberAcerto()
    {
        if (tipoMonstro == Temperamento.Rato_PassivoAgressivo) estaBravo = true;
        if (tipoMonstro == Temperamento.Galinha_PassivoCovarde) estaComMedo = true;
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

    private void OnDrawGizmosSelected()
    {
        Vector3 centro = Application.isPlaying ? pontoRespawn : transform.position;
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(centro, raioVadiagem);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(centro, raioPerseguicao);
        Gizmos.color = Color.blue; Gizmos.DrawWireSphere(transform.position, raioVisao);
    }
}