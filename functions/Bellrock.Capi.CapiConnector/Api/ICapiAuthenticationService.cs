using Bellrock.Capi.CapiConnector.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bellrock.Capi.CapiConnector.Api
{
    public interface ICapiAuthenticationService
    {
        Task<CapiAuthToken> GetTokenAsync();
        Task<CapiAuthToken> RefreshTokenAsync();
    }
}
