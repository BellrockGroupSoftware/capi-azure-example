using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bellrock.Capi.CapiConnector.Models
{
    public class CapiAuthToken
    {
        public string TokenScheme
        {
            get
            {
                return "X-CONCERTO-TOKEN";
            }
        }

        public string Token { get; set; } = string.Empty;
    }
}
