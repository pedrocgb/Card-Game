public static class CommandParser
{
    public static bool TryGetInt(string[] args, int index, out int value)
    {
        value = 0;
        if (args.Length <= index)
        {
            Console.Log("Missing argument at index " + index);
            return false;
        }

        if (!int.TryParse(args[index], out value))
        {
            Console.Log($"Argument '{args[index]}' is not a valid integer.");
            return false;
        }

        return true;
    }

    public static bool TryGetFloat(string[] args, int index, out float value)
    {
        value = 0;
        if (args.Length <= index)
        {
            Console.Log("Missing argument at index " + index);
            return false;
        }

        if (!float.TryParse(args[index], out value))
        {
            Console.Log($"Argument '{args[index]}' is not a valid integer.");
            return false;
        }

        return true;
    }

    public static string GetString(string[] args, int index)
    {
        if (args.Length <= index)
        {
            Console.Log("Missing string argument");
            return null;
        }

        return args[index];
    }
}
