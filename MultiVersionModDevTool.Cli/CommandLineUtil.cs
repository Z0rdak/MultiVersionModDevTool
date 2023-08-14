using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace de.z0rdak.moddev.util;

internal static class CommandLineUtil
{
    public static void ClearCurrentLine()
    {
        var currentLine = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLine);
    }

    public static void Loop()
    {
        var data = new List<string>
        {
            "Bar",
            "Barbec",
            "Barbecue",
            "Batman"
        };

        var builder = new StringBuilder();
        var input = Console.ReadKey(true);

        while (input.Key != ConsoleKey.Enter)
        {
            // get directory files
            var currentInput = builder.ToString();
            if (input.Key == ConsoleKey.Tab)
            {
                var match = data.FirstOrDefault(item =>
                    item != currentInput && item.StartsWith(currentInput, true, CultureInfo.InvariantCulture));
                if (string.IsNullOrEmpty(match))
                {
                    input = Console.ReadKey(true);
                    continue;
                }

                ClearCurrentLine();
                builder.Clear();

                Console.Write(match);
                builder.Append(match);
            }
            else
            {
                if (input.Key == ConsoleKey.Backspace && currentInput.Length > 0)
                {
                    builder.Remove(builder.Length - 1, 1);
                    ClearCurrentLine();

                    currentInput = currentInput.Remove(currentInput.Length - 1);
                    Console.Write(currentInput);
                }
                else
                {
                    var key = input.KeyChar;
                    builder.Append(key);
                    Console.Write(key);
                }
            }

            input = Console.ReadKey(true);
        }

        Console.Write(input.KeyChar);
    }
}