using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;

public class GerenteConsole : MonoBehaviour
{
    public static GerenteConsole instancia;
    [SerializeField] private TextMeshProUGUI textoLog;
    [SerializeField] private TMP_InputField inputField;

    public static event Action<string> AcaoMensagemEnviada;
    private List<string> historicoMensagens = new List<string>();

    void Awake() { instancia = this; }

    void Start()
    {
        if (inputField != null)
        {
            inputField.onSubmit.AddListener(delegate { EnviarPeloEnter(); });
            inputField.DeactivateInputField();
        }
    }

    void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (!inputField.isFocused)
            {
                inputField.ActivateInputField();
            }
        }
    }

    private void EnviarPeloEnter()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
            string textoDigitado = inputField.text;
            EscreverNoConsole("VOCÊ: " + textoDigitado);
            AcaoMensagemEnviada?.Invoke(textoDigitado);

            inputField.text = "";
        }

        // CORREÇÃO: Força a saída do foco e desativa o input
        inputField.DeactivateInputField();
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
    }

    public void EscreverNoConsole(string mensagem)
    {
        historicoMensagens.Add(mensagem);
        if (historicoMensagens.Count > 3) { historicoMensagens.RemoveAt(0); }

        textoLog.text = "";
        foreach (string frase in historicoMensagens) { textoLog.text += frase + "\n"; }
    }

    public bool EstaDigitando() { return inputField.isFocused; }
}