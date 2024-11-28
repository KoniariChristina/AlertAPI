using System.ComponentModel.DataAnnotations;

namespace AlertAPI.Models
{

   
    public class AlertIPAddress
    {       
        public int AlertID { get; set; }
        public Alert Alert { get; set; }        
        public string IPString { get; set; }
        public IPAddress IPAddress { get; set; }
        public int count {  get; set; }
    }
}
