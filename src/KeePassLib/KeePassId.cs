using KeePassLib;
using System;
using System.Diagnostics;

namespace KeePass
{
    [DebuggerDisplay("KeePass ID: {Id}")]
    public class KeePassId
    {
        private static readonly KeePassId s_empty = new KeePassId(string.Empty);

        public KeePassId(object id)
        {
            Id = id;
        }

        /// <summary>
        /// The object containing the Id
        /// </summary>
        /// <remarks>
        /// This must be a public property as the DTO between views serializes to JSON and requires this field
        /// </remarks>
        public object Id { get; set; }

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
            return Id.ToString();
        }

        public static implicit operator KeePassId(string id)
        {
            return string.IsNullOrEmpty(id) ? s_empty : new KeePassId(id);
        }

        public static explicit operator string(KeePassId id)
        {
            return id.Id.ToString();
        }

        public static implicit operator KeePassId(PwUuid id)
        {
            return new KeePassId(new Guid(id.UuidBytes));
        }

        public static implicit operator PwUuid(KeePassId id)
        {
            if (id.Id.GetType() != typeof(Guid))
            {
                if (id.IsEmpty)
                {
                    return new PwUuid(Guid.Empty.ToByteArray());
                }

                throw new InvalidOperationException();
            }

            return new PwUuid(((Guid)id.Id).ToByteArray());
        }

        public static KeePassId Empty => s_empty;
    }
}
