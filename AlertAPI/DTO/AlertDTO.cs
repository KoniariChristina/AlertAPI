using AlertAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace AlertAPI.DTO
{
    [Serializable]
    public class AlertDTO
    {      
        public string Title { get; set; }
        public string Description { get; set; }
        public int Severity { get; set; }
        public List<AlertIPAddressDTO> AlertsIPAddresses { get; set; }

    }  
}
