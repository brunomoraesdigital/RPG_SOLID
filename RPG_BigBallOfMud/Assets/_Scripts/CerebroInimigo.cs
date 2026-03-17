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
    [SerializeField] float raioVadiagem = 3f;
    [SerializeField] float raioVisao = 4f;
    [SerializeField] float raioPerseguicao = 8f;
    [SerializeField] float alcanceAtaque = 1.5f; // Aumentado para 1.5 para evitar o empurrão físico

    private bool estaBravo = false;
    private bool estaComMedo = false;
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

        // Mantém as restrições para não rotacionar sozinho
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
            FugirDoJogador();
        }
        else if ((tipoMonstro == Temperamento.Lobo_Agressivo && distJogador <= raioVisao && distJogadorProSpawn <= raioPerseguicao) ||
                 (estaBravo && distJogadorProSpawn <= raioPerseguicao))
        {
            // CORREÇÃO: Para o agente um pouco antes para evitar o empurrão
            if (distJogador <= alcanceAtaque)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero; // Zera a velocidade para travar no lugar
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
            if (estaComMedo && distInimigoProSpawn <= 1.0f && distJogador > raioVisao) estaComMedo = false;
            if (estaBravo && distJogadorProSpawn > raioPerseguicao) estaBravo = false;
            Patrulhar();
        }

        AjustarRotacaoVisual();
    }

    void ExecutarAtaqueMonstro()
    {
        cronometroAtaque += Time.deltaTime;
        if (cronometroAtaque >= 1f)
        {
            GameObject bastao = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Destroy(bastao.GetComponent<MeshCollider>());
            bastao.transform.SetParent(this.transform);

            // CORREÇÃO: Z em -0.1f para garantir que fique por cima de tudo
            bastao.transform.localPosition = new Vector3(0.8f, 0, -0.1f);
            bastao.transform.localRotation = Quaternion.identity;
            bastao.transform.localScale = new Vector3(1f, 0.2f, 1f);

            Renderer rend = bastao.GetComponent<Renderer>();
            rend.material.color = Color.red;
            rend.sortingOrder = 10; // Garante que apareça acima do jogador

            Destroy(bastao, 0.2f);
            cronometroAtaque = 0;
        }
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