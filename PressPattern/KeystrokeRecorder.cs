using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace PressPattern
{
    public class KeystrokeRecorder
    {
        private readonly string _user;
        private readonly string _csvPath;

        private readonly HashSet<string> _recordedKeys = new();
        private readonly HashSet<string> _targetKeys;

        private string _previousKey = null;
        private double _previousKeyUpTime = -1;
        private double _previousKeyDownTime = -1;
        private int _keyIndex = 0;
        private Stopwatch _sessionTimer;

        public KeystrokeRecorder(string user, string csvPath = "keystroke_data.csv")
        {
            _user = user;
            _csvPath = csvPath;
            _targetKeys = InitializeTargetKeySet();
        }

        public void Start()
        {
            Console.WriteLine($"[INFO] Start typing. Press ESC to stop.\n");

            _sessionTimer = Stopwatch.StartNew();

            while (true)
            {
                Console.Write("Press any key... ");
                var keyStopwatch = Stopwatch.StartNew();

                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                var keyDownTime = keyStopwatch.Elapsed;
                var sessionTime = _sessionTimer.Elapsed.TotalMilliseconds;

                while (Console.KeyAvailable)
                    Console.ReadKey(intercept: true);

                var keyUpTime = keyStopwatch.Elapsed;
                keyStopwatch.Stop();

                double duration = (keyUpTime - keyDownTime).TotalMilliseconds;
                string key = keyInfo.Key.ToString();

                double flightTime = _previousKeyUpTime >= 0
                    ? keyDownTime.TotalMilliseconds - _previousKeyUpTime
                    : 0;

                double latencyDownDown = _previousKeyDownTime >= 0
                    ? keyDownTime.TotalMilliseconds - _previousKeyDownTime
                    : 0;

                string bigram = _previousKey != null
                    ? $"{_previousKey}+{key}"
                    : key;

                int keyIndex = _keyIndex++;
                bool isRightHand = IsRightHandKey(key);

                AppendToCsv(key, keyDownTime.TotalMilliseconds, keyUpTime.TotalMilliseconds,
                            duration, flightTime, latencyDownDown, bigram, sessionTime, keyIndex, isRightHand);

                _previousKey = key;
                _previousKeyUpTime = keyUpTime.TotalMilliseconds;
                _previousKeyDownTime = keyDownTime.TotalMilliseconds;

                if (!_recordedKeys.Contains(key))
                    _recordedKeys.Add(key);

                ShowProgressBar();

                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\n[INFO] Recording stopped.");
                    break;
                }
            }
        }

        private void AppendToCsv(string key, double keyDown, double keyUp, double duration,
                                 double flightTime, double latencyDownDown, string bigram,
                                 double timeSinceStart, int keyIndex, bool isRightHand)
        {
            string line = string.Format(CultureInfo.InvariantCulture,
                "{0},{1:F2},{2:F2},{3:F2},{4:F2},{5:F2},{6},{7:F2},{8},{9},{10}",
                key, keyDown, keyUp, duration, flightTime, latencyDownDown,
                bigram, timeSinceStart, keyIndex, isRightHand, _user);

            File.AppendAllText(_csvPath, line + Environment.NewLine);
        }

        private bool IsRightHandKey(string key)
        {
            // uproszczone podejście, klawisze przypisane do prawej ręki na QWERTY
            string[] rightHandKeys = new[] {
                "Y", "U", "I", "O", "P", "H", "J", "K", "L", "N", "M",
                "RightShift", "RightCtrl", "RightAlt", "Enter", "Delete", "Backspace"
            };

            return Array.Exists(rightHandKeys, k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        private void ShowProgressBar()
        {
            int recorded = _recordedKeys.Count;
            int total = _targetKeys.Count;
            double percent = (double)recorded / total;
            int barWidth = 30;
            int filled = (int)(barWidth * percent);

            string bar = "[" + new string('█', filled) + new string('░', barWidth - filled) + $"] {percent * 100:F1}%";
            Console.WriteLine($"Key added: {recorded}/{total} {bar}");
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
