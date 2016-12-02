namespace KeePass
{
    public interface IClipboard<T>
    {
        bool Copy(T obj);
    }
}
