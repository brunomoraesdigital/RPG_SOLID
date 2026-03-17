using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CerebroNPC : MonoBehaviour
{
    public enum TipoNPC { Aventureiro, Mercador, AssistenteGuilda }
    [Header("Configurações do NPC")]
    public TipoNPC tipoDesteNPC;

    private int contadorTeste = 0;
    [SerializeField] Transform JOGADOR;
    private NavMeshAgent agent;

    private Vector3 pontoRespawn;
    [SerializeField] float raioVadiagem = 2f;
    [SerializeField] float raioVisao = 3f;

    [SerializeField] float tempoEsperaPatrulha = 3f;
    private float cronometroPatrulha;
    private bool jaFalou = false;
    private bool encerrouConversaForce = false;

    private int ultimaDicaIndice = -1;
    private float cronometroResposta = 0f;
    private bool aguardandoResposta = false;

    void OnEnable() { GerenteConsole.AcaoMensagemEnviada += OuvirJogador; }
    void OnDisable() { GerenteConsole.AcaoMensagemEnviada -= OuvirJogador; }

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

        float distanciaProJogador = Vector2.Distance(transform.position, JOGADOR.position);

        if (distanciaProJogador <= raioVisao)
        {
            if (!jaFalou && !encerrouConversaForce)
            {
                ExecutarPerguntaInicial();
                jaFalou = true;
            }

            if (aguardandoResposta)
            {
                agent.isStopped = true;
                OlharSuaveParaOJogador(); // Só olha enquanto aguarda sim/não/palavra-chave

                cronometroResposta += Time.deltaTime;
                if (cronometroResposta >= 15f)
                {
                    FicarBravoPorDemora();
                }
            }
            else
            {
                // Se não está aguardando (conversa acabou), volta a vadiar mesmo dentro do raio
                agent.isStopped = false;
                ExecutarVadiagemNPC();
                AjustarRotacaoVisual();
            }
        }
        else
        {
            // Reset ao sair do raio
            jaFalou = false;
            encerrouConversaForce = false;
            aguardandoResposta = false;
            agent.isStopped = false;
            ExecutarVadiagemNPC();
            AjustarRotacaoVisual();
        }
    }

    void ExecutarPerguntaInicial()
    {
        string mensagem = "";
        aguardandoResposta = true;
        cronometroResposta = 0f;

        switch (tipoDesteNPC)
        {
            case TipoNPC.Aventureiro: mensagem = "AVENTUREIRO: Deseja uma dica para sobreviver nestas terras?"; break;
            case TipoNPC.Mercador: mensagem = "MERCADOR: Pelas barbas de Odin! Olhe essas mercadorias. Deseja trocar seu ouro por algo útil?"; break;
            case TipoNPC.AssistenteGuilda: mensagem = "ASSISTENTE: Bem-vindo à representação da Guilda. Procuras por novas missões ou suporte?"; break;
        }

        if (GerenteConsole.instancia != null) GerenteConsole.instancia.EscreverNoConsole(mensagem);
    }

    void OuvirJogador(string texto)
    {
        float distancia = Vector2.Distance(transform.position, JOGADOR.position);
        if (distancia > raioVisao) return;

        // Gatilho de Voz: Se o jogador falar qualquer coisa vadiando, reinicia
        if (encerrouConversaForce || !aguardandoResposta)
        {
            encerrouConversaForce = false;
            jaFalou = false;
            return;
        }

        string msg = texto.ToLower();
        string resposta = "";

        if (tipoDesteNPC == TipoNPC.Mercador)
        {
            if (msg == "sim") { resposta = "MERCADOR: Excelente escolha! Veja o brilho destas espadas, a flexibilidade desse arco, o poder desse cajado e a pureza destas poções. O que vai levar?"; }
            else if (msg == "não" || msg == "nao") { resposta = "MERCADOR: Pois bem... ouro parado não compra glória. Volte quando decidir investir em sua sobrevivência."; FinalizarConversa(); }
            else if (msg.Contains("espada") || msg.Contains("cajado") || msg.Contains("flecha") || msg.Contains("arco") || msg.Contains("poção"))
            {
                resposta = "MERCADOR: Isso custa 50 ouros! Mas espere... você não tem um tostão! Saia daqui e só volte quando tiver ouro!";
                FinalizarConversa(); // Volta a vadiar após dar o valor
            }
            else { resposta = "MERCADOR: Pelos deuses, pare de balbuciar! Tempo é ouro e você me faz perder ambos. Suma da minha frente!"; FinalizarConversa(); }
        }
        else if (tipoDesteNPC == TipoNPC.AssistenteGuilda)
        {
            if (msg == "sim") { resposta = "ASSISTENTE: A Guilda agradece sua disposição. Tenho contratos de caça e escolta disponíveis. Qual seu interesse?"; }
            else if (msg == "não" || msg == "nao") { resposta = "ASSISTENTE: Entendo. Estarei aqui caso mude de ideia. Que a sorte acompanhe seus passos."; FinalizarConversa(); }
            else if (msg.Contains("caça")) { string[] monstros = { "Lobos", "Ratos", "Galinhas" }; resposta = "ASSISTENTE: O dever chama! Vá caçar " + monstros[Random.Range(0, 3)] + " e honre seu nome!"; FinalizarConversa(); }
            else if (msg.Contains("escolta")) { resposta = "ASSISTENTE: Precisamos de um grupo para missões de escolta. Volte quando não estiver sozinho."; FinalizarConversa(); }
            else { resposta = "ASSISTENTE: Se não veio em busca de trabalho, por favor, não obstrua a recepção."; FinalizarConversa(); }
        }
        else if (tipoDesteNPC == TipoNPC.Aventureiro)
        {
            if (msg == "sim" || msg.Contains("dica"))
            {
                string[] dicas = {
                    "Ouça bem... lobos cercam o norte. Mantenha o aço afiado!",
                    "Cuidado com os ratos. Eles parecem calmos, mas atacam se chegar perto!",
                    "Não perca tempo com as galinhas. Vão fugir antes de você atacar.",
                    "Coelhos são as criaturas mais rápidas e medrosas. Correm ao menor sinal."
                };
                int novoIndice; do { novoIndice = Random.Range(0, dicas.Length); } while (novoIndice == ultimaDicaIndice);
                ultimaDicaIndice = novoIndice;
                resposta = "AVENTUREIRO: " + dicas[novoIndice];
                FinalizarConversa(); // Volta a vadiar após dar a dica
            }
            else if (msg == "não" || msg == "nao") { resposta = "AVENTUREIRO: A arrogância é o primeiro passo para o túmulo. Siga seu caminho, então."; FinalizarConversa(); }
            else { resposta = "AVENTUREIRO: Suas palavras são confusas como um mapa borrado. Vá treinar e volte quando souber o que perguntar!"; FinalizarConversa(); }
        }

        if (GerenteConsole.instancia != null && resposta != "") GerenteConsole.instancia.EscreverNoConsole(resposta);
    }

    void FicarBravoPorDemora()
    {
        string bravo = "";
        switch (tipoDesteNPC)
        {
            case TipoNPC.Mercador: bravo = "MERCADOR: O tempo está passando e meu ouro não está aumentando. Pare de gastar meu fôlego!"; break;
            case TipoNPC.AssistenteGuilda: bravo = "ASSISTENTE: Tenho uma fila de heróis de verdade esperando. Saia da frente!"; break;
            case TipoNPC.Aventureiro: bravo = "AVENTUREIRO: Se vai ficar aí parado como uma estátua, que os corvos te façam companhia. Adeus!"; break;
        }
        if (GerenteConsole.instancia != null) GerenteConsole.instancia.EscreverNoConsole(bravo);
        FinalizarConversa();
    }

    void FinalizarConversa()
    {
        aguardandoResposta = false;
        encerrouConversaForce = true;
        agent.isStopped = false;
        cronometroResposta = 0f;
    }

    void OlharSuaveParaOJogador()
    {
        Vector3 direcao = (JOGADOR.position - transform.position).normalized;
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angulo), Time.deltaTime * 5f);
    }

    void ExecutarVadiagemNPC()
    {
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
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(centro, raioVadiagem);
        Gizmos.color = Color.blue; Gizmos.DrawWireSphere(transform.position, raioVisao);
    }
}