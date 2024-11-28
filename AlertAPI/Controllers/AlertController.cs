using AlertAPI.Models;
using AlertAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AlertAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertController : Controller
    {
        private readonly IAlertRepository _alertRepository;
        public AlertController(IAlertRepository alertRepository) { 
            _alertRepository = alertRepository;
        }

        [HttpGet]
        [Route("GetAlerts")]
        public async Task<IActionResult> GetAlerts()
        {
            var response = await _alertRepository.getAllAlerts();
            if (response != null)
            {
                return Ok(response);
            }

            return NotFound();
        }

        [HttpDelete]
        [Route("DeleteAlert")]
        public async Task<IActionResult> DeleteAlert(int alertID)
        {

            await _alertRepository.Delete(alertID);

            return Ok();



        }

        [HttpPut]
        [Route("UpdateAlertDescription")]
        public async Task<IActionResult> UpdateAlertDescription(int alertID, string description)
        {
            var alertDTO = await _alertRepository.UpdateDescription(alertID, description);
            if (alertDTO == null)
            {
                return NotFound();
            }

            return Ok(alertDTO);
        }

        [HttpPost]
        [Route("AddNewAlert")]
        public async Task<IActionResult> AddNewAlert(string title,string description,int severity,string ipString)
        {
            var alertDTO = await _alertRepository.AddNewAlert(title,description,severity,ipString);
            if (alertDTO == null)
            {
                return NotFound();
            }

            return Ok(alertDTO);
        }

        [HttpGet]
        [Route("PercentOfInternalIPs")]
        public async Task<IActionResult> PercentOfInternalIPs()
        {
            try
            {
                var percent = await _alertRepository.PercentOfInternalIPs();                
                return Ok(percent);
            }
            catch
            {
                return NotFound();
            }

        }
    }
}
