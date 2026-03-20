public class UnityConsoleLogger : ILogger
{
    public void Log(string message)
    {
        if (GerenteConsole.instancia != null)
            GerenteConsole.instancia.EscreverNoConsole(message);
    }
}