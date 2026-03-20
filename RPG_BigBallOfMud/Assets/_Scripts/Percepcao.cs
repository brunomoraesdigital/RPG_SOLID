using UnityEngine;

public class Percepcao : IPercepcao
{
    private Transform inimigo;
    private Transform jogador;
    private Vector3 pontoRespawn;
    private ConfiguracaoInimigo config;

    public Percepcao(Transform inimigo, Transform jogador, Vector3 pontoRespawn, ConfiguracaoInimigo config)
    {
        this.inimigo = inimigo;
        this.jogador = jogador;
        this.pontoRespawn = pontoRespawn;
        this.config = config;
    }

    public float DistanciaJogador => Vector3.Distance(inimigo.position, jogador.position);
    public float DistanciaJogadorAteSpawn => Vector3.Distance(pontoRespawn, jogador.position);
    public float DistanciaInimigoAteSpawn => Vector3.Distance(inimigo.position, pontoRespawn);
    public bool JogadorNoRaioVisao => DistanciaJogador <= config.raioVisao;
    public bool JogadorNoRaioPerseguicao => DistanciaJogadorAteSpawn <= config.raioPerseguicao;
    public bool JogadorNoAlcanceAtaque => DistanciaJogador <= config.alcanceAtaque;
    public Vector3 PosicaoJogador => jogador.position;
}