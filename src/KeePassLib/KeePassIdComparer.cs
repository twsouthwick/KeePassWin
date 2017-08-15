using System.Collections.Generic;

namespace KeePass
{
    public class KeePassIdComparer : IEqualityComparer<IKeePassId>
    {
        public static KeePassIdComparer Instance { get; } = new KeePassIdComparer();

        public bool Equals(IKeePassId x, IKeePassId y) => Equals(x?.Id, y?.Id);

        public int GetHashCode(IKeePassId obj) => obj?.Id.GetHashCode() ?? 0;
    }
}
