namespace KeePass
{
    public struct KeePassEntryWithParent
    {
        internal KeePassEntryWithParent(IKeePassEntry entry, IKeePassGroup group)
        {
            Entry = entry;
            Parent = group;
        }

        public IKeePassEntry Entry { get; }

        public IKeePassGroup Parent { get; }
    }
}
