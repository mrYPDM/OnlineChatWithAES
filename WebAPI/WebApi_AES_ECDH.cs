using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI
{
    namespace SecureWebApi
    {
        public abstract class WebApi_AES_ECDH : WebAPI
        {
            protected Aes aes;
            protected byte[] KeyExchange(IConnection otherParty)
            {
                if (!otherParty.Connected) throw new Exception("otherParty is disconnected");

                ECDiffieHellmanCng dh = new(256)
                {
                    KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                    HashAlgorithm = CngAlgorithm.Sha256
                };

                otherParty.SendMessage(new Message("key_exchanger", Convert.ToBase64String(dh.PublicKey.ToByteArray()), "key").ToBase64String());

                Message otherParty_key_message = Message.FromBase64String(otherParty.ReadMessage());
                if (otherParty_key_message == null) throw new Exception("Received null key");

                byte[] otherParty_key_bytes = Convert.FromBase64String(otherParty_key_message.Text);
                CngKey otherParty_key = CngKey.Import(otherParty_key_bytes, CngKeyBlobFormat.EccPublicBlob);
                return dh.DeriveKeyMaterial(otherParty_key);
            }

            public WebApi_AES_ECDH() : base()
            {
                aes = Aes.Create();
            }
        }
    }
}
