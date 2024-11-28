using AlertAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace AlertAPI.DTO
{
    [Serializable]
    public class AlertIPAddressDTO
    { 
        public IPAddressDTO IPAddress { get; set; }

    }
}
