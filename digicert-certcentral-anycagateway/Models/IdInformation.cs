using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyfactor.Extensions.CAPlugin.DigiCert.Models
{
    public class IdInformation
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [DefaultValue(null)]
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
