using KeePassLib;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace KeePass
{
    public class KeePassId
    {
        private static readonly KeePassId s_empty = new KeePassId(Guid.Empty);

        public KeePassId(Guid id)
        {
            Id = id;
        }

        /// <summary>
        /// The object containing the Id
        /// </summary>
        /// <remarks>
        /// This must be a public property as the DTO between views serializes to JSON and requires this field
        /// </remarks>
        public Guid Id { get; }

        public bool IsEmpty => ReferenceEquals(s_empty, this);

        public override bool Equals(object obj)
        {
            var other = obj as KeePassId;

            if (other == null)
            {
                return false;
            }

            return Equals(other.Id, Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            var s = Id.ToString("N").ToUpperInvariant().ToCharArray();
            for (int i = 0; i < s.Length; i += 2)
            {
                sb.Append(s[i]);
                sb.Append(s[i + 1]);

                if (i + 2 != s.Length)
                {
                    sb.Append('-');
                }
            }

            return sb.ToString();
        }

        public static explicit operator string(KeePassId id)
        {
            return id.Id.ToString();
        }

        public static KeePassId Empty => s_empty;
    }
}
