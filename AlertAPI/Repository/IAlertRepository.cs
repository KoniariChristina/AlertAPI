using AlertAPI.DTO;
using AlertAPI.Models;

namespace AlertAPI.Repository
{
    public interface IAlertRepository
    {
        Task<List<AlertDTO>> getAllAlerts();       
        Task Delete(int alertID);
        Task<AlertDTO> UpdateDescription(int alertID, string description);
        Task<AlertDTO> AddNewAlert(string title, string description, int severity, string ipString);
        Task<decimal> PercentOfInternalIPs();
    }
}
