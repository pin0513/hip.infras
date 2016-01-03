namespace hip.Infrastructure.Interfaces.EncryptionProvider
{
    /// <summary>
    /// 雜湊加密(不可逆)
    /// </summary>
    public interface IHashingEncryptionProvider
    {
        string ComputeHash(string originString);
    }
}
