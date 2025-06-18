using System.Collections.Generic;
using UnityEngine;

public class CommandProcessor : MonoBehaviour
{
    #region Variables and Properties
    public static CommandProcessor Instance = null;

    private Dictionary<string, ConsoleCommand> _commands = new Dictionary<string, ConsoleCommand>();
    #endregion

    // ========================================================================

    #region Static Methods
    public static void ExecuteCommand(string Input)
    {
        if (Instance == null)
            return;

        Instance.executeCommand(Input);
    }
    #endregion

    // ========================================================================

    #region Initialization Methods
    private void Awake()
    {
        if (Instance == null)
            Instance = this;    
        
    }
    private void Start()
    {
        RegisterBuiltInCommands();
    }
    #endregion

    // ========================================================================

    #region Commands Methods
    public void executeCommand(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        string[] parts = input.Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        string commandName = parts[0].ToLower();
        string[] args = parts.Length > 1 ? parts[1..] : new string[0];

        if (_commands.TryGetValue(commandName, out var cmd))
        {
            cmd.Execute.Invoke(args);
        }
        else
        {
            Console.Log($"Command '{commandName}' not found. Type 'help' for a list of commands.");
        }
    }

    private void RegisterCommand(ConsoleCommand cmd)
    {
        _commands[cmd.Name.ToLower()] = cmd;
    }

    private void RegisterBuiltInCommands()
    {
        //// List all available commands
        RegisterCommand(new ConsoleCommand(
            "help",
            "List all commands",
            args =>
            {
                foreach (var c in _commands.Values)
                {
                    Console.Log($"{c.Name}: {c.Description}");
                }
            }));

        //// Draw cards commands
        // Draws 1 card
        RegisterCommand(new ConsoleCommand(
           "drawcard",
           "Draws a card for current Actor's turn.",
           args =>
           {
               CombatManager.Instance.CurrentCombatent.Hand.DrawCards(1);
               Console.Log($"Drew 1 card for {CombatManager.Instance.CurrentCombatent.ActorName}.");
           }));
        // Draws X cards
        RegisterCommand(new ConsoleCommand(
           "drawcards",
           "Draws X cards for current Actor's turn. Must be an integer greater than 0, ex.: drawcards 2",
           args =>
           {
               if (!CommandParser.TryGetInt(args, 0, out int amount)) return;

               CombatManager.Instance.CurrentCombatent.Hand.DrawCards(amount);
               Console.Log($"Drew {amount} cards for {CombatManager.Instance.CurrentCombatent.ActorName}.");
           }));
    }
    #endregion

    // ========================================================================
}
