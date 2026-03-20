using UnityEngine;

public interface IComportamentoIA
{
    void Executar(CerebroInimigo cerebro, IMovimento movimento, IAtaque ataque, ILogger logger, EstadoInimigo estado, Temperamento tipo, IPercepcao percepcao, ConfiguracaoInimigo config);
}

public class ComportamentoFugir : IComportamentoIA
{
    public void Executar(CerebroInimigo cerebro, IMovimento movimento, IAtaque ataque, ILogger logger, EstadoInimigo estado, Temperamento tipo, IPercepcao percepcao, ConfiguracaoInimigo config)
    {
        if (!estado.MensagemDisparada && tipo == Temperamento.Coelho_Covarde)
        {
            logger.Log("O coelho iniciou a fuga desesperada pulando o mais rapido que pode!");
            estado.MarcarMensagemDisparada(); // Usa método em vez de atribuição direta
        }

        Vector3 direcaoOposta = cerebro.GetTransform().position - percepcao.PosicaoJogador;
        Vector3 destinoFuga = cerebro.GetTransform().position + direcaoOposta.normalized * 5f;
        movimento.MoverPara(destinoFuga);
    }
}

public class ComportamentoPerseguir : IComportamentoIA
{
    public void Executar(CerebroInimigo cerebro, IMovimento movimento, IAtaque ataque, ILogger logger, EstadoInimigo estado, Temperamento tipo, IPercepcao percepcao, ConfiguracaoInimigo config)
    {
        if (!estado.MensagemDisparada && tipo == Temperamento.Lobo_Agressivo)
        {
            logger.Log("O lobo iniciou a perseguicao com as suas presas a mostra!");
            estado.MarcarMensagemDisparada(); // Usa método em vez de atribuição direta
        }

        movimento.RotacionarPara(percepcao.PosicaoJogador);

        if (percepcao.JogadorNoAlcanceAtaque)
        {
            movimento.Parar();
            ataque.Atacar(cerebro.GetTransform(), tipo.ToString().Split('_')[0], logger);
        }
        else
        {
            movimento.MoverPara(percepcao.PosicaoJogador);
        }
    }
}

public class ComportamentoPatrulhar : IComportamentoIA
{
    private float cronometroPatrulha;
    private float tempoEsperaPatrulha;

    // Construtor recebe o tempo de espera e inicializa o cronômetro
    public ComportamentoPatrulhar(float tempoEsperaPatrulha)
    {
        this.tempoEsperaPatrulha = tempoEsperaPatrulha;
        this.cronometroPatrulha = tempoEsperaPatrulha;
    }

    public void Executar(CerebroInimigo cerebro, IMovimento movimento, IAtaque ataque, ILogger logger, EstadoInimigo estado, Temperamento tipo, IPercepcao percepcao, ConfiguracaoInimigo config)
    {
        // Resets de estado
        if (estado.MensagemDisparada)
        {
            if ((estado.EstaComMedo || tipo == Temperamento.Coelho_Covarde) && !percepcao.JogadorNoRaioVisao)
            {
                if (tipo == Temperamento.Galinha_PassivoCovarde && estado.EstaComMedo)
                    logger.Log("A galinha saiu da sua visao, ela esta voltando pro seu ninho.");
                if (tipo == Temperamento.Coelho_Covarde)
                    logger.Log("O coelho conseguiu sair da sua visao, ele esta voltando pra sua toca.");

                estado.ResetarFuga();
            }
            else if (!percepcao.JogadorNoRaioPerseguicao || (tipo == Temperamento.Lobo_Agressivo && !percepcao.JogadorNoRaioVisao))
            {
                if (tipo == Temperamento.Lobo_Agressivo)
                    logger.Log("O lobo desistiu da perseguicao com ar de satisfacao.");
                if (tipo == Temperamento.Rato_PassivoAgressivo)
                    logger.Log("O rato desistiu da perseguicao com ar indignado.");

                estado.ResetarPerseguicao();
            }
        }

        // Patrulha
        if (movimento.EstaParado)
        {
            cronometroPatrulha -= Time.deltaTime;
            if (cronometroPatrulha <= 0)
            {
                Vector2 p = Random.insideUnitCircle * config.raioVadiagem;
                Vector3 destino = cerebro.PontoRespawn + new Vector3(p.x, p.y, 0);
                movimento.MoverPara(destino);
                cronometroPatrulha = tempoEsperaPatrulha;
            }
        }
        else
        {
            // Se não está parado, reinicia o cronômetro
            cronometroPatrulha = tempoEsperaPatrulha;
        }
    }
}