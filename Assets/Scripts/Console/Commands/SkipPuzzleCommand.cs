using System.Collections.Generic;
using System.Reflection;
using AbstractConsole;

[ConsoleCommand("skip", "s")]
public class SkipPuzzleCommand : IConsoleCommand
{
    public string Name => "skip";
    public string Description => "Saltar jogo ativo.";
    public string[] Aliases => new string[] { "s" };
    public string Usage => "skip [command]";

    InteractableGame[] games;

    public void InjectData(object data)
    {
        if (data is InteractableGame[] list)
            games = list;
    }

    public void Execute(IConsole runtime, string[] args)
    {
        if (games.Length <= 0)
        {
            runtime.Log($"Nenhum jogo encontrado.", ConsoleLogType.Warning, "console");
            return;
        }

        foreach (InteractableGame game in games)
        {
            if (game.gameObject.activeSelf)
            {
                game.ForceFinish();
                runtime.Log($"Jogo {game.name} finalizado.", ConsoleLogType.Log, "console");

                return;
            }
        }

        runtime.Log($"Nenhum jogo ativo.", ConsoleLogType.Log, "console");

        return;
    }
}
