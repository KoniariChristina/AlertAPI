using AlertAPI.Data;
using AlertAPI.DTO;
using AlertAPI.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;


namespace AlertAPI.Repository
{
    public class AlertRepository : IAlertRepository
    {

        private readonly AlertAPIDBContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IMapper _mapper;
        private readonly ILogger<AlertRepository> _logger;
        private const int blacklistThreshold = 2;
        public AlertRepository(AlertAPIDBContext dbContext, IHttpClientFactory httpClientFactory, IServiceScopeFactory ScopeFactory, IMapper mapper, ILogger<AlertRepository> logger)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _scopeFactory = ScopeFactory;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<List<AlertDTO>> getAllAlerts()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MyClient");

                var isFirstTime = true;
                int currentPage = 1;
                int totalPages = 10;
                int pageSize = 1;

                while (currentPage <= totalPages)
                {
                    if (isFirstTime)
                    {
                        var response = await client.GetAsync("/api/alerts/b"); //take the first page

                        string responseBody = await response.Content.ReadAsStringAsync();
                        StoreData(responseBody);

                        if (response.Headers.TryGetValues("X-Pagination", out var paginationValues))
                        {
                            var paginationHeader = paginationValues.First();
                            var paginationInfo = JsonSerializer.Deserialize<Pagination>(paginationHeader);

                            if (paginationInfo != null)
                            {
                                totalPages = paginationInfo.page_count;
                                currentPage = paginationInfo.page_no;
                                pageSize = paginationInfo.page_size;
                            }
                        }

                        isFirstTime = false;

                    }
                    else
                    {
                        var response = await client.GetAsync(string.Format("/api/alerts/a?page_size={0}&page_no={1}", pageSize, currentPage));
                        string responseBody = await response.Content.ReadAsStringAsync();
                        StoreData(responseBody);

                    }

                    currentPage++;
                }

                var alertList = await _dbContext.Alerts.ToListAsync();
                if (alertList != null)
                {
                    List<AlertDTO> alertDTOList = new List<AlertDTO>();
                    foreach (Alert alert in alertList)
                    {
                        var alertDTO = _mapper.Map<AlertDTO>(alert);
                        alertDTOList.Add(alertDTO);
                    }

                    return alertDTOList;
                }

                return null;
            }
            catch
            {
                _logger.LogError("Error in getAllAlerts");
                throw new Exception("Error in getAllAlerts");
            }




        }

        public void StoreData(string responseBody)
        {

            try
            {
                var data = JsonSerializer.Deserialize<List<AlertJSON>>(responseBody);


                foreach (AlertJSON alertJson in data)
                {
                    // List<Alert> sameAlert = null;
                    Alert sameAlert = null;
                    //check if title and severity exist
                    if (_dbContext.Alerts.Count() > 0)
                    {
                        sameAlert = _dbContext.Alerts.Where(x => (x.Title == alertJson.title)).FirstOrDefault();
                    }


                    if (sameAlert != null)//already exist same alert
                    {
                        foreach (var ip in alertJson.ips) //for each that we receive
                        {
                            var sameAlertWithSameIPAddress = _dbContext.AlertIPAddresses.Where(x => (x.AlertID == sameAlert.ID) && (x.IPString == ip)).FirstOrDefault();
                            if (sameAlertWithSameIPAddress != null) //already exist same alert with the same ip
                            {
                                sameAlertWithSameIPAddress.count += 1;
                                if (sameAlertWithSameIPAddress.count > blacklistThreshold)
                                {
                                    Models.IPAddress ipAddress = _dbContext.IPAddresses.Find(ip);
                                    ipAddress.Blacklisted = true;
                                }
                            }
                            else //the alert exist but not for the same ip
                            {
                                //insert only the ip if is needed and assing the count value 
                                HandleInsertOfIP(ip, sameAlert);
                            }
                        }

                    }
                    else //we dont have the same alert
                    {
                        InsertNewAlert(alertJson);
                    }
                }



            }
            catch (Exception ex)
            {
                _logger.LogError("Error in data storage");
                throw new Exception("Error in data storage");
            }


        }

        public void InsertNewAlert(AlertJSON alertJSON)
        {

            var alert = new Alert()
            {
                Title = alertJSON.title,
                Description = alertJSON.description,
                Severity =(int)(object) alertJSON.severity,
                // Severity = SeveretyName(alertJSON.severity),
            };


            foreach (string ip in alertJSON.ips)
            {
                //chech if we have this ip in database
                HandleInsertOfIP(ip, alert);

            }
        }

        public void HandleInsertOfIP(string ip, Alert alert)
        {
            var ipInDatabase = _dbContext.IPAddresses.Find(ip);

            //the case: existed alert with this ip is handled above

            if (ipInDatabase != null) //ip exists in database
            {
                var alertsIPAddress = new AlertIPAddress()
                {
                    AlertID = alert.ID,
                    Alert = alert,
                    IPString = ip,
                    IPAddress = ipInDatabase,
                    count = 1
                };

                _dbContext.AlertIPAddresses.Add(alertsIPAddress);
                _dbContext.SaveChanges();
            }
            else
            {
                var ipAddress = new Models.IPAddress()
                {
                    IPString = ip,
                    Blacklisted = false,
                    sourceType = CheckSourceType(ip)
                };

                var alertsIPAddress = new AlertIPAddress()
                {
                    AlertID = alert.ID,
                    Alert = alert,
                    IPString = ip,
                    IPAddress = ipAddress,
                    count = 1
                };

                _dbContext.AlertIPAddresses.Add(alertsIPAddress);
                _dbContext.SaveChanges();

            }

        }      

        public Models.SourceType CheckSourceType(string IP)
        {
            if (System.Net.IPAddress.TryParse(IP, out System.Net.IPAddress? address))
            {
                byte[] bytes = address.GetAddressBytes();
                if (bytes[0] == 10)
                    return Models.SourceType.Internal;

                // 172.16.0.0 to 172.31.255.255
                if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                    return Models.SourceType.Internal;

                // 192.168.0.0 to 192.168.255.255
                if (bytes[0] == 192 && bytes[1] == 168)
                    return Models.SourceType.Internal;

                return Models.SourceType.External;

            }
            _logger.LogError("Error in IP source type check");
            throw new Exception("Error in IP source type check");

        }

        public async Task Delete(int alertID)
        {
            var alert = await _dbContext.Alerts.FindAsync(alertID);

            if (alert != null)
            {
                _dbContext.Alerts.Remove(alert);
                await _dbContext.SaveChangesAsync();

            }
            else
            {
                _logger.LogError("This alert doesn't exist");
                throw new Exception("This alert doesn't exist");
            }

        }

        public async Task<AlertDTO> UpdateDescription(int alertID, string description)
        {
            var alert = await _dbContext.Alerts.FindAsync(alertID);

            if (alert == null)
            {
                return null;
            }

            alert.Description = description;

            await _dbContext.SaveChangesAsync();

            var alertDTO = _mapper.Map<AlertDTO>(alert);

            return alertDTO;
        }

        public async Task<AlertDTO> AddNewAlert(string title, string description, int severity, string ipString)
        {
            Alert sameAlert = await _dbContext.Alerts.Where(x => (x.Title == title)).FirstOrDefaultAsync();
            if (sameAlert == null)
            {
                //add new alert
                var alert = new Alert()
                {
                    Title = title,
                    Description = description,
                    Severity = severity,

                };

                HandleInsertOfIP(ipString, alert);

                var alertDTO = _mapper.Map<AlertDTO>(alert);
                return alertDTO;

            }
            else
            {
                //if alert exist

                var sameAlertWithSameIPAddress = await _dbContext.AlertIPAddresses.Where(x => (x.AlertID == sameAlert.ID) && (x.IPString == ipString)).FirstOrDefaultAsync();
                if (sameAlertWithSameIPAddress != null) //already exist same alert with the same ip
                {
                    sameAlertWithSameIPAddress.count += 1;
                    if (sameAlertWithSameIPAddress.count > blacklistThreshold)
                    {
                        Models.IPAddress ipAddress = await _dbContext.IPAddresses.FindAsync(ipString);
                        if (ipAddress != null)
                        {
                            ipAddress.Blacklisted = true;
                        }

                    }
                }
                else //the alert exist but not for the same ip
                {
                    //insert only the ip if is needed and assing the count value 
                    HandleInsertOfIP(ipString, sameAlert);
                }

                var alertDTO = _mapper.Map<AlertDTO>(sameAlert);
                return alertDTO;

            }


        }

        public async Task<decimal> PercentOfInternalIPs()
        {
            try
            {
                var allIPs = await _dbContext.IPAddresses.CountAsync();
                var internalIPs = await _dbContext.IPAddresses.Where(x => x.sourceType == 0).ToListAsync();
                if (allIPs != 0)
                {
                    var percent = (Decimal.Divide(internalIPs.Count(), allIPs)) * 100;
                    return Math.Round(percent, 2);
                }
                else
                {
                    _logger.LogError("There is any IP Address");
                    throw new Exception("There is any IP Address");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error in the calculation of internal IPs percentage");
                throw new Exception("Error in the calculation of internal IPs percentage");
            }



        }
    }
    [Serializable]
    public class Pagination
    {
        public int total_record_count { get; set; }
        public int page_size { get; set; }
        public int page_no { get; set; }
        public int page_count { get; set; }
    }

    [Serializable]
    public class AlertJSON
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }

        [JsonConverter(typeof(StringOrIntConverter))]
        public object severity { get; set; }
        public List<string> ips { get; set; }
    }

    public class StringOrIntConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32(); 
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (value == "very low")
                {
                    return 0;
                }
                else if (value == "low")
                {
                    return 1;
                }
                else if (value == "high")
                {
                    return 2;
                }
               
            }
            throw new JsonException("Unexpected token type");
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value is int intValue)
            {
                writer.WriteNumberValue(intValue);
            }
            else if (value is string strValue)
            {
                if ((string)(object)value == "very low")
                {
                    writer.WriteNumberValue(0);
                }
                else if ((string)(object)value == "low")
                {
                    writer.WriteNumberValue(1);
                }
                else if ((string)(object)value == "high")
                {
                    writer.WriteNumberValue(2);
                }
               
            }
            else
            {
                throw new JsonException("Unexpected value type");
            }
        }
    }
}
