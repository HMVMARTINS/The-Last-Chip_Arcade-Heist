using System.Collections.Generic;
using System.Reflection;
using AbstractConsole;
using UnityEngine;

[ConsoleCommand("help", "?")]
public class HelpCommand : IConsoleCommand
{
    public string Name => "help";
    public string Description => "Lista comandos registrados.";
    public string[] Aliases => new string[] { "?" };
    public string Usage => "help [command]";

    public void InjectData(object data) { }

    public void Execute(IConsole runtime, string[] args)
    {
        if (runtime is ConsoleCore core)
        {
            if (args.Length == 0)
            {
                foreach (
                    var c in core.GetType()
                        .GetField("commands", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(core) as Dictionary<string, IConsoleCommand>
                )
                {
                    runtime.Log($"{c.Key} - {c.Value.Description}", ConsoleLogType.Log, "console");
                }
            }
            else
            {
                var name = args[0];
                // Try to find and print usage
                var dict =
                    core.GetType()
                        .GetField("commands", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(core) as Dictionary<string, IConsoleCommand>;
                if (dict.TryGetValue(name, out var cmd))
                    runtime.Log(
                        $"{cmd.Name}: {cmd.Description}\nUsage: {cmd.Usage}",
                        ConsoleLogType.Log,
                        "console"
                    );
                else
                    runtime.Log($"Command not found: {name}", ConsoleLogType.Warning, "console");
            }
        }
    }
}
