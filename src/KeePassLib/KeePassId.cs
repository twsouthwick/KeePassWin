using System;
using System.Text;

namespace KeePass
{
    public struct KeePassId
    {
        public static KeePassId Empty { get; } = new KeePassId(Guid.Empty);

        public KeePassId(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }

        public bool IsEmpty => Id == Guid.Empty;

        public override bool Equals(object obj)
        {
            if (!(obj is KeePassId))
            {
                return false;
            }

            return Equals(((KeePassId)obj).Id, Id);
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
            return id.ToString();
        }
    }
}
