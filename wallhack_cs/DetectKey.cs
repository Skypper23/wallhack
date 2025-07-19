using ImGuiNET;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace GlobalKey
{
    public class GlobalKeyDetector
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public static int LastKey { get; private set; }

        public static void Start()
        {
            _hookID = SetHook(_proc);
            if (_hookID == IntPtr.Zero)
            {
                Console.WriteLine("Falha ao instalar o hook.");
            }
            else
            {
                Console.WriteLine("Hook instalado com sucesso.");
            }
        }


        public static void Stop()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                LastKey = vkCode;

                string nomeVK = VKCodeToName(vkCode);
                Console.WriteLine($"Tecla: {vkCode} - {nomeVK}");
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public static string VKCodeToName(int code)
        {
            return code switch
            {
                0x08 => "VK_BACK",
                0x09 => "VK_TAB",
                0x0D => "VK_RETURN",
                0x10 => "VK_SHIFT",
                0x11 => "VK_CONTROL",
                0x12 => "VK_MENU (ALT)",
                0x13 => "VK_PAUSE",
                0x14 => "VK_CAPITAL (CAPS LOCK)",
                0x1B => "VK_ESCAPE",
                0x20 => "VK_SPACE",
                0x21 => "VK_PRIOR (PAGE UP)",
                0x22 => "VK_NEXT (PAGE DOWN)",
                0x23 => "VK_END",
                0x24 => "VK_HOME",
                0x25 => "VK_LEFT",
                0x26 => "VK_UP",
                0x27 => "VK_RIGHT",
                0x28 => "VK_DOWN",
                0x2C => "VK_SNAPSHOT (PRINT SCREEN)",
                0x2D => "VK_INSERT",
                0x2E => "VK_DELETE",
                >= 0x30 and <= 0x39 => $"VK_{(char)code}", // 0-9
                >= 0x41 and <= 0x5A => $"VK_{(char)code}", // A-Z
                0x70 => "VK_F1",
                0x71 => "VK_F2",
                0x72 => "VK_F3",
                0x73 => "VK_F4",
                0x74 => "VK_F5",
                0x75 => "VK_F6",
                0x76 => "VK_F7",
                0x77 => "VK_F8",
                0x78 => "VK_F9",
                0x79 => "VK_F10",
                0x7A => "VK_F11",
                0x7B => "VK_F12",
                0x5B => "VK_LWIN",
                0x5C => "VK_RWIN",
                0x5D => "VK_APPS",
                0x90 => "VK_NUMLOCK",
                0x91 => "VK_SCROLL",
                _ => $"VK_UNKNOWN ({code})"
            };
        }


        public static ImGuiKey VKCodeToImGuiKey(int vkCode)
        {
            return vkCode switch
            {
                // Teclas básicas
                0x08 => ImGuiKey.Backspace,
                0x09 => ImGuiKey.Tab,
                0x0D => ImGuiKey.Enter,
                0x10 => ImGuiKey.LeftShift,     // Shift genérico
                0x11 => ImGuiKey.LeftCtrl,      // Ctrl genérico
                0x12 => ImGuiKey.LeftAlt,       // Alt genérico
                0x1B => ImGuiKey.Escape,
                0x20 => ImGuiKey.Space,
                0x2E => ImGuiKey.Delete,

                // Teclas de seta
                0x25 => ImGuiKey.LeftArrow,
                0x26 => ImGuiKey.UpArrow,
                0x27 => ImGuiKey.RightArrow,
                0x28 => ImGuiKey.DownArrow,

                // Teclas alfanuméricas (A-Z, 0-9)
                >= 0x41 and <= 0x5A => (ImGuiKey)(vkCode - 0x41 + (int)ImGuiKey.A), // A-Z
                >= 0x30 and <= 0x39 => (ImGuiKey)(vkCode - 0x30 + (int)ImGuiKey._0), // 0-9

                // Teclas de função (F1-F24)
                >= 0x70 and <= 0x87 => (ImGuiKey)(vkCode - 0x70 + (int)ImGuiKey.F1),

                // Teclado numérico
                0x60 => ImGuiKey.Keypad0,
                0x61 => ImGuiKey.Keypad1,
                // ... (adicione até Keypad9, KeypadDecimal, etc.)

                // Teclas de modificação específicas (Left/Right)
                0xA0 => ImGuiKey.LeftShift,
                0xA1 => ImGuiKey.RightShift,
                0xA2 => ImGuiKey.LeftCtrl,
                0xA3 => ImGuiKey.RightCtrl,
                0xA4 => ImGuiKey.LeftAlt,
                0xA5 => ImGuiKey.RightAlt,

                // Teclas especiais
                0x5B => ImGuiKey.LeftSuper,     // Tecla Windows esquerda
                0x5C => ImGuiKey.RightSuper,    // Tecla Windows direita
                0x2C => ImGuiKey.PrintScreen,
                0x91 => ImGuiKey.ScrollLock,
                0x13 => ImGuiKey.Pause,
                0x14 => ImGuiKey.CapsLock,

                // Teclas não mapeadas no ImGuiKey
                0x1C => ImGuiKey.Enter,        // Numpad Enter (tratado como Enter normal)
                0x90 => ImGuiKey.NumLock,

                // Fallback para teclas desconhecidas
                _ => ImGuiKey.None
            };
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn,
            IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
