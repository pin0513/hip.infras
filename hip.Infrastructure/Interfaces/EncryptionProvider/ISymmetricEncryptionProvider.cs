namespace hip.Infrastructure.Interfaces.EncryptionProvider
{
    /// <summary>
    /// 對稱加密
    /// </summary>
    public interface ISymmetricEncryptionProvider
    {
        string EncryptingData(string originString);
        string DecryptingData(string encryptedString);
    }
}
