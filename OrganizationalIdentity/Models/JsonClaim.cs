using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrganizationalIdentity.Models
{
    public interface IJsonClaim
    {
        Claim Claim { get; }
        object Value { get; }
        string Type { get; }
    }

    public class JsonClaim : IJsonClaim
    {
        public Claim Claim { get; private set; }
        public dynamic Value
        {
            get
            {
                var type = Claim.ValueType.Replace("Json:", String.Empty);
                return JsonConvert.DeserializeObject(Claim.Value, System.Type.GetType(type)); ;
            }
        }
        public string Type => Claim.Type;

        public JsonClaim(Claim claim)
        {
            if (claim == null) throw new ArgumentNullException("claim");
            if (!claim.ValueType.StartsWith("Json:"))
                throw new ArgumentException("Claim has unsupported ValueType", "claim");
            Claim = claim;
        }

        public JsonClaim(string type, object value)
        {
            Claim = new Claim(type, JsonConvert.SerializeObject(value), $"Json:{value.GetType().AssemblyQualifiedName}");
        }

        public JsonClaim(string type, object value, string issuer)
        {
            Claim = new Claim(type, JsonConvert.SerializeObject(value), $"Json:{value.GetType().AssemblyQualifiedName}", issuer);
        }

        public JsonClaim(string type, object value, string issuer, string originalIssuer)
        {
            Claim = new Claim(type, JsonConvert.SerializeObject(value), $"Json:{value.GetType().AssemblyQualifiedName}", issuer, originalIssuer);
        }

        public JsonClaim(string type, object value, string issuer, string originalIssuer, ClaimsIdentity subject)
        {
            Claim = new Claim(type, JsonConvert.SerializeObject(value), $"Json:{value.GetType().AssemblyQualifiedName}", issuer, originalIssuer, subject);
        }

    }

}
