using UnityEngine;

public interface ILogger
{
    void Log(string message);
}

public interface IMovimento
{
    void MoverPara(Vector3 destino);
    void Parar();
    void RotacionarPara(Vector3 alvo);
    void RotacionarParaVelocidade();
    Vector3 Posicao { get; }
    bool EstaParado { get; }
}

public interface IPercepcao
{
    float DistanciaJogador { get; }
    float DistanciaJogadorAteSpawn { get; }
    float DistanciaInimigoAteSpawn { get; }
    bool JogadorNoRaioVisao { get; }
    bool JogadorNoRaioPerseguicao { get; }
    bool JogadorNoAlcanceAtaque { get; }
    Vector3 PosicaoJogador { get; }
}

public interface IAtaque
{
    void Atacar(Transform atacante, string nomeInimigo, ILogger logger);
    void AtualizarCooldown();
    bool PodeAtacar { get; }
}