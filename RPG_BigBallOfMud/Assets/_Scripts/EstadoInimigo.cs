public class EstadoInimigo
{
    public bool EstaBravo { get; private set; }
    public bool EstaComMedo { get; private set; }
    public bool MensagemDisparada { get; private set; }

    public void AtivarFuria()
    {
        if (!EstaBravo)
        {
            EstaBravo = true;
            MensagemDisparada = true;
        }
    }

    public void AtivarMedo()
    {
        if (!EstaComMedo)
        {
            EstaComMedo = true;
            MensagemDisparada = true;
        }
    }

    public void ResetarFuga()
    {
        EstaComMedo = false;
        MensagemDisparada = false;
    }

    public void ResetarPerseguicao()
    {
        EstaBravo = false;
        MensagemDisparada = false;
    }

    // Método público para marcar que a mensagem foi disparada
    public void MarcarMensagemDisparada()
    {
        if (!MensagemDisparada)
            MensagemDisparada = true;
    }
}