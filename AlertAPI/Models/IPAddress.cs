using System.ComponentModel.DataAnnotations;

namespace AlertAPI.Models
{

   
    public class IPAddress
    {
        [Key]      
        public string IPString { get; set; }
        public bool Blacklisted { get; set; }
        public SourceType sourceType { get; set; }
        public  List<AlertIPAddress> AlertsIPAddresses { get; set; }
    }


    public enum SourceType
    {
        Internal =0 ,
        External =1
    }
}
