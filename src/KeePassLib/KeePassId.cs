using KeePassLib;
using System;
using System.Diagnostics;

namespace KeePass
{
    [DebuggerDisplay("KeePass ID: {Id}")]
    public class KeePassId
    {
        private static readonly KeePassId s_empty = new KeePassId(string.Empty);

        private readonly object _id;

        public KeePassId(object id)
        {
            _id = id;
        }

        public bool IsEmpty => ReferenceEquals(s_empty, this);

        public override bool Equals(object obj)
        {
            var other = obj as KeePassId;

            if (other == null)
            {
                return false;
            }

            return Equals(other._id, _id);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override string ToString()
        {
            return _id.ToString();
        }

        public static implicit operator KeePassId(string id)
        {
            return string.IsNullOrEmpty(id) ? s_empty : new KeePassId(id);
        }

        public static explicit operator string(KeePassId id)
        {
            return id._id.ToString();
        }

        public static implicit operator KeePassId(int id)
        {
            return new KeePassId(id.ToString());
        }

        public static implicit operator KeePassId(PwUuid id)
        {
            return new KeePassId(new Guid(id.UuidBytes).ToString());
        }

        public static implicit operator PwUuid(KeePassId id)
        {
            if (id._id.GetType() != typeof(Guid))
            {
                if (id.IsEmpty)
                {
                    return new PwUuid(Guid.Empty.ToByteArray());
                }

                throw new InvalidOperationException();
            }

            return new PwUuid(((Guid)id._id).ToByteArray());
        }

        public static KeePassId Empty => s_empty;
    }
}
