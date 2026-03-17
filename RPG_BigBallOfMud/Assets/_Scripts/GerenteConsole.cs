using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System; // Adicionado para usar o Action

public class GerenteConsole : MonoBehaviour
{
    public static GerenteConsole instancia;
    [SerializeField] private TextMeshProUGUI textoLog;
    [SerializeField] private TMP_InputField inputField;

    // A PONTE: Evento para os NPCs ouvirem
    public static event Action<string> AcaoMensagemEnviada;

    private List<string> historicoMensagens = new List<string>();

    void Awake()
    {
        instancia = this;
    }

    void Start()
    {
        if (inputField != null)
        {
            inputField.onSubmit.AddListener(delegate { EnviarPeloEnter(); });
        }
    }

    void Update() { }

    private void EnviarPeloEnter()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            string textoDigitado = inputField.text;
            EscreverNoConsole("VOCÊ: " + textoDigitado);

            // GRITA PARA O MUNDO: NPCs escutam aqui
            AcaoMensagemEnviada?.Invoke(textoDigitado);

            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    public void EscreverNoConsole(string mensagem)
    {
        historicoMensagens.Add(mensagem);
        if (historicoMensagens.Count > 3) { historicoMensagens.RemoveAt(0); }

        textoLog.text = "";
        foreach (string frase in historicoMensagens)
        {
            textoLog.text += frase + "\n";
        }
    }

    public bool EstaDigitando()
    {
        return inputField != null && inputField.isFocused;
    }
}