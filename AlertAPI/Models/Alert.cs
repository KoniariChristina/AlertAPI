using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Numerics;

namespace AlertAPI.Models
{
   
    public class Alert
    {
        [Key]
        public  int ID { get; set; }
        public  string  Title { get; set; }
        public string Description { get; set; }
        public int Severity { get; set; }
        public List<AlertIPAddress> AlertsIPAddresses { get; set; }
        
    }

    public enum SeverityEnum
    {
        Very_Low =0,
        Low =1,
        High =2

    }
}
