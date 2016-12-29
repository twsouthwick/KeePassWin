using System;
using System.Collections.Generic;
using System.Text;

namespace KeePass
{
    public abstract class KeyboardShortcuts
    {
        private readonly Dictionary<KeyInfo, ShortcutName> _keys = new Dictionary<KeyInfo, ShortcutName>();

        public string GetShortcutString(int key)
        {
            return GetKeyInfo(key).ToString();
        }

        public string GetShortcutString(ShortcutName name)
        {
            foreach (var item in _keys)
            {
                if (item.Value == name)
                {
                    return item.Key.ToString(KeyToString);
                }
            }

            return string.Empty;
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

        public IEnumerable<ShortcutName> ShortcutNames => _keys.Values;

        public bool UpdateKey(ShortcutName name, int key)
        {
            var info = GetKeyInfo(key);

            return UpdateKey(name, info);
        }

        protected bool UpdateKey(ShortcutName name, KeyInfo info)
        {
            if (info.Key == 0)
            {
                return false;
            }

            // Don't overwrite another shortcut 
            if (_keys.ContainsKey(info))
            {
                return false;
            }

            foreach (var item in _keys)
            {
                if (item.Value == name)
                {
                    _keys.Remove(item.Key);
                    break;
                }
            }

            _keys[info] = name;

            return true;
        }

        protected abstract KeyInfo GetKeyInfo(int key);

        protected virtual string KeyToString(int key)
        {
            return key.ToString();
        }

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
