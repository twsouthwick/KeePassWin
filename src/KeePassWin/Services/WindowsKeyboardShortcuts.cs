using System.Collections.Generic;
using Windows.System;

namespace KeePass.Win.Services
{
    public class WindowsKeyboardShortcuts : KeyboardShortcuts
    {
        private static readonly Dictionary<ShortcutName, KeyInfo> s_default = new Dictionary<ShortcutName, KeyInfo>
        {
            { ShortcutName.Close, new KeyInfo(Modifier.Menu, (int)VirtualKey.F4) },
            { ShortcutName.Search, new KeyInfo(Modifier.Control, (int)VirtualKey.E) },
            { ShortcutName.CopyUserName, new KeyInfo(Modifier.Control, (int)VirtualKey.C) },
            { ShortcutName.CopyPassword, new KeyInfo(Modifier.Control, (int)VirtualKey.B) },
            { ShortcutName.OpenUrl, new KeyInfo(Modifier.Control | Modifier.Shift, (int)VirtualKey.U) },
            { ShortcutName.CopyUrl, new KeyInfo(Modifier.Control, (int)VirtualKey.U) },
            { ShortcutName.InsertItem, new KeyInfo(Modifier.Control, (int)VirtualKey.I) },
            { ShortcutName.ViewItem, new KeyInfo(Modifier.None, (int)VirtualKey.Enter) },
            { ShortcutName.DuplicateItem, new KeyInfo(Modifier.Control, (int)VirtualKey.K) },
            { ShortcutName.DeleteItem, new KeyInfo(Modifier.None, (int)VirtualKey.Delete) },
            { ShortcutName.ShowMenu, new KeyInfo(Modifier.None, (int)VirtualKey.Application) },
            { ShortcutName.Rename, new KeyInfo(Modifier.None, (int)VirtualKey.F2) }
        };

        public WindowsKeyboardShortcuts()
        {
            foreach (var @default in s_default)
            {
                UpdateKey(@default.Key, @default.Value);
            }
        }

        protected override KeyInfo GetKeyInfo(int key)
        {
            var modifier = GetModifier(VirtualKey.Shift, Modifier.Shift)
             | GetModifier(VirtualKey.Control, Modifier.Control)
             | GetModifier(VirtualKey.Menu, Modifier.Menu)
             | GetModifier(VirtualKey.Shift, Modifier.Shift);

            return new KeyInfo(modifier, key);
        }

        private static Modifier GetModifier(VirtualKey key, Modifier modifier)
        {
            return Windows.UI.Xaml.Window.Current.CoreWindow.GetKeyState(key).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down)
                ? modifier : Modifier.None;
        }
    }
}
