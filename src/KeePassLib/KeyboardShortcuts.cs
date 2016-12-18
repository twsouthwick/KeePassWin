using System;
using System.Collections.Generic;
using System.Text;

namespace KeePass
{
    public abstract class KeyboardShortcuts
    {
        private readonly Dictionary<KeyInfo, ShortcutName> _keys = new Dictionary<KeyInfo, ShortcutName>();
        private readonly Dictionary<ShortcutName, KeyInfo> _names = new Dictionary<ShortcutName, KeyInfo>();

        public string GetShortcutString(int key)
        {
            return GetKeyInfo(key).ToString();
        }

        public string GetShortcutString(ShortcutName name)
        {
            KeyInfo info;
            if (_names.TryGetValue(name, out info))
            {
                return info.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        public ShortcutName ProcessKeypress(int key)
        {
            var info = GetKeyInfo(key);

            ShortcutName name;
            if (_keys.TryGetValue(info, out name))
            {
                return name;
            }
            else
            {
                return ShortcutName.None;
            }
        }

        protected void UpdateKey(ShortcutName name, KeyInfo info)
        {
            if (_names.ContainsKey(name))
            {
                _names.Remove(name);
                _keys.Remove(info);
            }

            _keys.Add(info, name);
            _names.Add(name, info);
        }

        protected abstract KeyInfo GetKeyInfo(int key);

        [Flags]
        protected enum Modifier
        {
            None = 0,
            Control = 1,
            Menu = 2,
            Shift = 4
        }

        protected struct KeyInfo
        {
            private readonly Modifier _modifier;

            public KeyInfo(Modifier modifier, int key)
            {
                _modifier = modifier;
                Key = key;
            }

            public int Key { get; }

            public override string ToString() => ToString(i => i.ToString());

            public string ToString(Func<int, string> keyFormatter)
            {
                var sb = new StringBuilder();

                if (_modifier.HasFlag(Modifier.Control))
                {
                    sb.Append("CTRL+");
                }

                if (_modifier.HasFlag(Modifier.Menu))
                {
                    sb.Append("ALT+");
                }

                if (_modifier.HasFlag(Modifier.Shift))
                {
                    sb.Append("Shift+");
                }

                sb.Append(keyFormatter(Key));

                return sb.ToString();
            }
        }
    }
}
