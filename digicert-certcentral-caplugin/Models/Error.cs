using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAPlugin.DigiCert.Models
{
    public class Error
    {
        [JsonProperty("code")]
        public string code { get; set; }

        [JsonProperty("message")]
        public string message { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(code)) return message ?? string.Empty;
            if (string.IsNullOrEmpty(message)) return code;
            return $"{code}: {message}";
        }
    }

    public class Errors
    {
        [JsonProperty("errors")]
        public List<Error> errors { get; set; }
    }
}
