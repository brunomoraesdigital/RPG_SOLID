using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem; // Sistema moderno

public class GerenteConsole : MonoBehaviour
{
    public static GerenteConsole instancia;
    [SerializeField] private TextMeshProUGUI textoLog;
    [SerializeField] private TMP_InputField inputField;

    private List<string> historicoMensagens = new List<string>();

    void Awake()
    {
        instancia = this;
    }

    void Start()
    {
        // Configura o campo para disparar a função ao apertar Enter
        if (inputField != null)
        {
            inputField.onSubmit.AddListener(delegate { EnviarPeloEnter(); });
        }
    }

    // Removemos a lógica do Update para usar o evento nativo (mais limpo e moderno)
    void Update() { }

    private void EnviarPeloEnter()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            EscreverNoConsole("VOCÊ: " + inputField.text);
            inputField.text = "";
            inputField.ActivateInputField(); // Mantém o foco para a próxima mensagem
        }
    }

    public void EscreverNoConsole(string mensagem)
    {
        // Alterado de 10 para 3 conforme solicitado
        historicoMensagens.Add(mensagem);
        if (historicoMensagens.Count > 3) { historicoMensagens.RemoveAt(0); }

        textoLog.text = "";
        foreach (string frase in historicoMensagens)
        {
            textoLog.text += frase + "\n";
        }
    }

    // Função para checar se o jogador está digitando (usaremos no CerebroJogador)
    public bool EstaDigitando()
    {
        return inputField != null && inputField.isFocused;
    }
}