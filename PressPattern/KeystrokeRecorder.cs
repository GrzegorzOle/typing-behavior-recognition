using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace PressPattern
{
    public class KeystrokeRecorder
    {
        private readonly string _user;
        private readonly string _csvPath;

        private readonly HashSet<string> _recordedKeys = new();
        private readonly HashSet<string> _targetKeys;

        public KeystrokeRecorder(string user, string csvPath = "keystroke_data.csv")
        {
            _user = user;
            _csvPath = csvPath;
            _targetKeys = InitializeTargetKeySet();
        }

        public void Start()
        {
            Console.WriteLine($"[INFO] Start typing. Press ESC to stop.\n");

            while (true)
            {
                Console.Write("Press any key... ");
                var stopwatch = Stopwatch.StartNew();

                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                var keyDownTime = stopwatch.Elapsed;

                while (Console.KeyAvailable)
                    Console.ReadKey(intercept: true);

                var keyUpTime = stopwatch.Elapsed;
                stopwatch.Stop();

                double duration = (keyUpTime - keyDownTime).TotalMilliseconds;
                string key = keyInfo.Key.ToString();

                AppendToCsv(key, keyDownTime.TotalMilliseconds, keyUpTime.TotalMilliseconds, duration);

                // Dodaj tylko jeśli nowy klawisz
                if (!_recordedKeys.Contains(key))
                {
                    _recordedKeys.Add(key);
                }

                ShowProgressBar(); // <-- Teraz ZAWSZE

                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\n[INFO] Recording stopped.");
                    break;
                }
            }
        }


        private void AppendToCsv(string key, double keyDown, double keyUp, double duration)
        {
            string line = string.Format(CultureInfo.InvariantCulture, "{0},{1:F2},{2:F2},{3:F2},{4}", key, keyDown, keyUp, duration, _user);
            File.AppendAllText(_csvPath, line + Environment.NewLine);
        }

        private void ShowProgressBar()
        {
            int recorded = _recordedKeys.Count;
            int total = _targetKeys.Count;
            double percent = (double)recorded / total;
            int barWidth = 30;
            int filled = (int)(barWidth * percent);

            string bar = "[" + new string('█', filled) + new string('░', barWidth - filled) + $"] {percent * 100:F1}%";
            Console.WriteLine($"Key added: {_recordedKeys.Count}/{total} {bar}");
        }

        private HashSet<string> InitializeTargetKeySet()
        {
            var keys = new HashSet<string>();

            for (char c = 'A'; c <= 'Z'; c++)
                keys.Add(c.ToString());

            for (int i = 0; i <= 9; i++)
                keys.Add(i.ToString());

            string[] specialKeys = new[]
            {
            "Spacebar", "Enter", "Backspace", "Tab",
            "Escape", "LeftArrow", "RightArrow", "UpArrow", "DownArrow",
            "LeftShift", "RightShift", "LeftCtrl", "RightCtrl", "LeftAlt", "RightAlt",
            "CapsLock", "Delete", "Insert", "Home", "End", "PageUp", "PageDown"
        };

            foreach (var key in specialKeys)
                keys.Add(key);

            return keys;
        }
    }
}
