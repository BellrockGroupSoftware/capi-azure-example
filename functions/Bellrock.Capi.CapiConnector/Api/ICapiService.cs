using Bellrock.Capi.CapiConnector.Models;

namespace Bellrock.Capi.CapiConnector.Api
{
    public interface ICapiService
    {
        Task<CapiOrder> GetOrderAsync(Guid orderId);
    }
}
