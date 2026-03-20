public class DecisorIA
{
    private IPercepcao percepcao;
    private EstadoInimigo estado;
    private ConfiguracaoInimigo config;
    private ILogger logger;
    private Temperamento tipo;
    private ComportamentoPatrulhar comportamentoPatrulha; // Instância única para manter o cronômetro

    public DecisorIA(IPercepcao percepcao, EstadoInimigo estado, ConfiguracaoInimigo config, ILogger logger, Temperamento tipo)
    {
        this.percepcao = percepcao;
        this.estado = estado;
        this.config = config;
        this.logger = logger;
        this.tipo = tipo;
        // Cria a instância de patrulha uma única vez, passando o tempo de espera
        this.comportamentoPatrulha = new ComportamentoPatrulhar(config.tempoEsperaPatrulha);
    }

    public IComportamentoIA Decidir()
    {
        bool deveFugir = (tipo == Temperamento.Coelho_Covarde && percepcao.JogadorNoRaioVisao) ||
                         (estado.EstaComMedo && percepcao.JogadorNoRaioVisao);

        if (deveFugir)
            return new ComportamentoFugir();

        bool devePerseguir = (tipo == Temperamento.Lobo_Agressivo && percepcao.JogadorNoRaioVisao && percepcao.JogadorNoRaioPerseguicao) ||
                             (estado.EstaBravo && percepcao.JogadorNoRaioPerseguicao);

        if (devePerseguir)
            return new ComportamentoPerseguir();

        return comportamentoPatrulha; // Retorna a mesma instância sempre
    }
}