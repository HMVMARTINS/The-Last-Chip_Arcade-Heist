using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AbstractConsole;
using UnityEngine;

public class ConsoleReferencer : MonoBehaviour
{
    [SerializeField]
    public List<InteractableGame> games;

    [SerializeField]
    ConsoleOverlay consoleOverlay;
    ConsoleCore consoleCore;

    void Start()
    {
        consoleCore = consoleOverlay.Core;

        // obtenha o dicionário real de comandos
        var dict =
            consoleCore
                .GetType()
                .GetField("_commands", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(consoleCore) as Dictionary<string, IConsoleCommand>;

        // injete os dados em todos os comandos que precisam
        foreach (var cmd in dict.Values)
            cmd.InjectData(games.ToArray());
    }
}
