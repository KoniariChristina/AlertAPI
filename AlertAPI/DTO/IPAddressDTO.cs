using AlertAPI.Models;

namespace AlertAPI.DTO
{
    [Serializable]
    public class IPAddressDTO
    {
        public string IPString { get; set; }
        public bool Blacklisted { get; set; }
        public SourceType sourceType { get; set; }
  
    }

        public enum SourceType
        {
            Internal = 0,
            External = 1
        }
    }
