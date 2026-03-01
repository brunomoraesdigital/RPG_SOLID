using UnityEngine;
using UnityEngine.InputSystem;

public class CerebroJogador : MonoBehaviour
{
    public enum TipoArma { Espada, Arco, Cajado }

    [Header("--- EQUIPAMENTO ---")]
    public TipoArma armaEquipada = TipoArma.Espada;
    public int atk_arma = 10;

    [Header("--- PROGRESSO E PONTOS ---")]
    [Range(1, 100)] public int NV = 1;
    public int pontosDisponiveis;

    [Header("--- ATRIBUTOS PRIMÁRIOS ---")]
    [Range(1, 100)] public int FOR = 1;
    [Range(1, 100)] public int AGI = 1;
    [Range(1, 100)] public int VIT = 1;
    [Range(1, 100)] public int INT = 1;
    [Range(1, 100)] public int DES = 1;
    [Range(1, 100)] public int SOR = 1;

    private int fOld, aOld, vOld, iOld, dOld, sOld, nvOld;

    [Header("--- STATUS SECUNDÁRIOS ---")]
    public int vida_max; public int mana_max;
    public int atk_fis, atk_mag, atk_dist, defesa;
    public int vel_ataque, esquiva, precisao, acerto_critico, esquiva_perfeita, carga_max;

    private Rigidbody2D rb;
    private Vector2 entrada;
    private float tempoReuso = 0;
    public float velocidadeBase = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SalvarEstadoAnterior();
        CalcularStatus();

        GameObject spawn = GameObject.Find("PontoDeSpawn");
        if (spawn != null) transform.position = spawn.transform.position;
    }

    void Update()
    {
        CalcularStatus();
        GerenciarInputs();
    }

    void OnValidate()
    {
        CalcularStatus();
    }

    void CalcularStatus()
    {
        int totalGanhos = NV - 1;
        int gastos = (FOR - 1) + (AGI - 1) + (VIT - 1) + (INT - 1) + (DES - 1) + (SOR - 1);

        if (gastos > totalGanhos)
        {
            FOR = fOld; AGI = aOld; VIT = vOld;
            INT = iOld; DES = dOld; SOR = sOld;
            NV = nvOld;
            gastos = (FOR - 1) + (AGI - 1) + (VIT - 1) + (INT - 1) + (DES - 1) + (SOR - 1);
        }
        else
        {
            SalvarEstadoAnterior();
        }

        pontosDisponiveis = totalGanhos - gastos;

        // Fórmulas com Atk Arma (Sempre Inteiros)
        atk_mag = (int)((INT * 4) + (DES / 2f) + SOR) + atk_arma;
        atk_fis = (int)((FOR * 2) + (DES / 2f) + SOR) + atk_arma;
        atk_dist = (int)(DES + (AGI / 2f) + SOR) + atk_arma;

        vel_ataque = (int)((AGI * 2) + DES);
        esquiva = (int)((AGI * 2) + (DES / 2f) + SOR);
        precisao = (int)((DES * 2) + (AGI / 2f) + SOR);
        acerto_critico = (int)(Mathf.Floor(SOR / 2f) * Mathf.Floor(DES / 10f));
        esquiva_perfeita = (int)(Mathf.Floor(SOR / 2f) * Mathf.Floor(AGI / 10f));
        vida_max = (int)((VIT * 3 + NV * 2) * 100);
        mana_max = (int)((INT * 3 + NV * 2) * 100);
        defesa = (int)((VIT * 3) + (FOR / 2f) + (NV * 2));
        carga_max = (int)(((FOR * 10) + NV) * 10);
    }

    void SalvarEstadoAnterior()
    {
        fOld = FOR; aOld = AGI; vOld = VIT;
        iOld = INT; dOld = DES; sOld = SOR;
        nvOld = NV;
    }

    void GerenciarInputs()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado == null) return;

        float x = (teclado.aKey.isPressed || teclado.leftArrowKey.isPressed) ? -1 : (teclado.dKey.isPressed || teclado.rightArrowKey.isPressed) ? 1 : 0;
        float y = (teclado.wKey.isPressed || teclado.upArrowKey.isPressed) ? 1 : (teclado.sKey.isPressed || teclado.downArrowKey.isPressed) ? -1 : 0;
        entrada = new Vector2(x, y);

        if (tempoReuso > 0) tempoReuso -= Time.deltaTime;
        if (tempoReuso <= 0)
        {
            if (teclado.jKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame) ProcessarAtaque(1);
            if (teclado.kKey.wasPressedThisFrame) ProcessarAtaque(2);
            if (teclado.lKey.wasPressedThisFrame) ProcessarAtaque(3);
        }
    }

    void ProcessarAtaque(int b)
    {
        if (armaEquipada == TipoArma.Espada)
        {
            if (b == 1) Debug.Log($"Golpe Rápido! Dano: {atk_fis}");
            if (b == 2) { Debug.Log($"Golpe Fulminante! Dano: {(int)(atk_fis * 1.5f)}"); tempoReuso = 1f; }
            if (b == 3) { Debug.Log($"Impacto Explosivo! Dano: {(int)(atk_fis * 2f)}"); tempoReuso = 3f; }
        }
        else if (armaEquipada == TipoArma.Arco)
        {
            if (b == 1) Debug.Log($"Tiro Rápido! Dano: {atk_dist}");
            if (b == 2) { Debug.Log($"Rajada de Flechas! Dano: {(int)(atk_dist * 1.5f)}"); tempoReuso = 1f; }
            if (b == 3) { Debug.Log($"Chuva de Flechas! Dano: {(int)(atk_dist * 2f)}"); tempoReuso = 3f; }
        }
        else if (armaEquipada == TipoArma.Cajado)
        {
            if (b == 1) Debug.Log($"Ataque de Mana! Dano: {atk_mag}");
            if (b == 2) { Debug.Log($"Bola de Fogo! Dano: {(int)(atk_mag * 1.8f)}"); tempoReuso = 3f; }
            if (b == 3) { Debug.Log($"Meteoro! Dano: {(int)(atk_mag * 3f)}"); tempoReuso = 3f; }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + entrada.normalized * velocidadeBase * Time.fixedDeltaTime);
    }
}