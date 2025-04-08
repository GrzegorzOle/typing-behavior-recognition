using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace InvisiTypeAI
{
    public class KeystrokeRecorder
    {
        private readonly Action<MLModelType.ModelInput> _onKeyProcessed;
        private readonly Dictionary<Key, Stopwatch> _activeKeys = new();
        private readonly Stopwatch _sessionTimer = new();
        private readonly HashSet<string> _targetKeys;
        private string? _previousKey = null;
        private double _previousKeyUpTime = -1;
        private double _previousKeyDownTime = -1;
        private int _keyIndex = 0;
        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelKeyboardProc? _proc;

        public bool IsRunning { get; private set; }

        public KeystrokeRecorder(Action<MLModelType.ModelInput> onKeyProcessed)
        {
            _onKeyProcessed = onKeyProcessed;
            _targetKeys = InitializeTargetKeySet();
        }

        public void Start()
        {
            if (IsRunning) return;
            _sessionTimer.Restart();
            _keyIndex = 0;

            _proc = HookCallback;
            _hookId = SetHook(_proc);
            IsRunning = true;
        }

        public void Stop()
        {
            if (!IsRunning) return;
            UnhookWindowsHookEx(_hookId);
            _sessionTimer.Stop();
            IsRunning = false;
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using var curProcess = Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule!;
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);
                string keyName = key.ToString();

                if (!_targetKeys.Contains(keyName))
                    return CallNextHookEx(_hookId, nCode, wParam, lParam);

                double currentTime = _sessionTimer.Elapsed.TotalMilliseconds;

                if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
                {
                    if (!_activeKeys.ContainsKey(key))
                    {
                        _activeKeys[key] = Stopwatch.StartNew();

                        _previousKeyDownTime = currentTime;
                    }
                }
                else if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
                {
                    if (_activeKeys.TryGetValue(key, out var sw))
                    {
                        sw.Stop();
                        double duration = sw.Elapsed.TotalMilliseconds;
                        _activeKeys.Remove(key);

                        double keyDownTime = currentTime - duration;
                        double keyUpTime = currentTime;
                        double flightTime = _previousKeyUpTime >= 0 ? keyDownTime - _previousKeyUpTime : 0;
                        double latency = _previousKeyDownTime >= 0 ? keyDownTime - _previousKeyDownTime : 0;
                        string bigram = $"{_previousKey ?? "Spacebar"}+{keyName}";
                        bool isRightHand = IsRightHandKey(keyName);
                        int keyIndex = _keyIndex++;

                        var input = new MLModelType.ModelInput
                        {
                            Col0 = keyName,
                            Col1 = (float)keyDownTime,
                            Col2 = (float)keyUpTime,
                            Col3 = (float)duration,
                            Col4 = (float)flightTime,
                            Col5 = (float)latency,
                            Col6 = bigram,
                            Col7 = (float)currentTime,
                            Col8 = keyIndex,
                            Col9 = isRightHand,
                            Col10 = ""
                        };

                        _previousKey = keyName;
                        _previousKeyUpTime = keyUpTime;

                        _onKeyProcessed?.Invoke(input);
                    }
                }
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private bool IsRightHandKey(string key)
        {
            string[] rightHandKeys = new[]
            {
                "Y", "U", "I", "O", "P", "H", "J", "K", "L", "N", "M",
                "RightShift", "RightCtrl", "RightAlt", "Enter", "Delete", "Backspace"
            };

            return Array.Exists(rightHandKeys, k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
        }

        private HashSet<string> InitializeTargetKeySet()
        {
            var keys = new HashSet<string>();
            for (char c = 'A'; c <= 'Z'; c++) keys.Add(c.ToString());
            for (int i = 0; i <= 9; i++) keys.Add(i.ToString());

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

        #region WinAPI
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);
        #endregion
    }
}
