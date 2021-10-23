using System;
using System.Security.Cryptography;
using WebAPI;

namespace TcpClientServerChat
{
    public abstract class SecureWebAPI : WebAPI.WebAPI
    {
        protected Aes aes;
        protected byte[] KeyExchange(Connection otherParty)
        {
            ECDiffieHellmanCng dh = new(256)
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.Sha256
            };

            otherParty.SendMessage(new Message("key_exchanger", Convert.ToBase64String(dh.PublicKey.ToByteArray()), "key").ToBase64String());

            Message otherParty_key_message = Message.FromBase64String(otherParty.ReadMessage());
            byte[] otherParty_key_bytes = Convert.FromBase64String(otherParty_key_message.Text);
            CngKey otherParty_key = CngKey.Import(otherParty_key_bytes, CngKeyBlobFormat.EccPublicBlob);
            return dh.DeriveKeyMaterial(otherParty_key);
        }

        public SecureWebAPI() : base()
        {
            aes = Aes.Create();
        }
    }
}
