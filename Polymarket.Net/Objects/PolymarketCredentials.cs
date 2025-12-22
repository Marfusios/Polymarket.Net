using CryptoExchange.Net.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polymarket.Net.Objects
{
    public class PolymarketCredentials : ApiCredentials
    {
        public string Address { get; set; }

        public PolymarketCredentials(string address, string privateKey) : base(address, privateKey, null, ApiCredentialsType.Hmac)
        {
            Address = address;
        }

        public PolymarketCredentials(string address, string key, string secret, string pass) : base(key, secret, pass, ApiCredentialsType.Hmac)
        {
            Address = address;
        }

        public override ApiCredentials Copy()
        {
            return new PolymarketCredentials(Address, Key, Secret, Pass)
            {
                CredentialType = this.CredentialType
            };
        }
    }
}
