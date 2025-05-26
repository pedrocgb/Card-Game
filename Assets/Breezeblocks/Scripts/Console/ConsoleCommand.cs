using UnityEngine;

public class ConsoleCommand
{
    public string Name;
    public string Description;
    public System.Action<string[]> Execute;

    // ========================================================================

    public ConsoleCommand(string name, string description, System.Action<string[]> execute)
    {
        Name = name;
        Description = description;
        Execute = execute;
    }

    // ========================================================================
}
