using UnityEngine;
using UnityEngine.AI;

public class CerebroNPC : MonoBehaviour
{
    private int contadorTeste = 0;
    [SerializeField] Transform JOGADOR;
    private NavMeshAgent agent;

    private Vector3 pontoRespawn;
    [SerializeField] float raioVadiagem = 2f; // Conforme seu pedido
    [SerializeField] float raioVisao = 3f;

    [SerializeField] float tempoEsperaPatrulha = 3f;
    private float cronometroPatrulha;
    private bool jaFalou = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        pontoRespawn = transform.position;
        cronometroPatrulha = tempoEsperaPatrulha;

        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
    }

    void Update()
    {
        if (JOGADOR == null) return;

        float distanciaProJogador = Vector2.Distance(transform.position, JOGADOR.position);

        // Lógica de conversa: Ao ver o jogador, ele para e fala
        if (distanciaProJogador <= raioVisao)
        {
            if (!jaFalou)
            {
                Debug.Log("NPC DIZ: Cuidado, aventureiro! Há lobos famintos nesta floresta.");
                contadorTeste++; // Aumenta 1 toda vez que entra no raio
                GerenteConsole.instancia.EscreverNoConsole("NPC: Cuidado! (Mensagem " + contadorTeste + ")");
                jaFalou = true;
            }

            // O NPC para de andar para conversar
            agent.isStopped = true;
        }
        else
        {
            // Se o jogador se afastar, ele volta a vadiar e pode falar de novo se você voltar
            agent.isStopped = false;
            jaFalou = false;
            ExecutarVadiagemNPC();
        }

        AjustarRotacaoVisual();
    }

    void ExecutarVadiagemNPC()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            cronometroPatrulha -= Time.deltaTime;
            if (cronometroPatrulha <= 0)
            {
                Vector2 pontoAleatorio = Random.insideUnitCircle * raioVadiagem;
                Vector3 destinoFinal = pontoRespawn + new Vector3(pontoAleatorio.x, pontoAleatorio.y, 0);

                NavMeshHit hit;
                if (NavMesh.SamplePosition(destinoFinal, out hit, 1.0f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
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
        Gizmos.color = Color.green; // NPC usa verde para patrulha
        Gizmos.DrawWireSphere(centro, raioVadiagem);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, raioVisao);
    }
}