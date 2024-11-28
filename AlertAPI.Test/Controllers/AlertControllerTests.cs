using AlertAPI.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using AlertAPI.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using AlertAPI.Models;

namespace AlertAPI.Test.Controllers
{
    public class AlertControllerTests
    {
        private readonly IAlertRepository _alertRepository;

        public AlertControllerTests()
        {
            _alertRepository = A.Fake<IAlertRepository>();
        }

        [Fact]
        public async void AlertController_GetAlerts_ReturnOK()
        {
            var controller = new AlertController(_alertRepository);

            var result = await controller.GetAlerts();

            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(OkObjectResult));
        }

        [Fact]
        public async void AlertController_DeleteAlert_ReturnOK()
        {
            int alertId = 1;
            var controller = new AlertController(_alertRepository);

            var result = await controller.DeleteAlert(alertId);

            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(OkResult));

        }

        [Fact]
        public async void AlertController_AddNewAlert_ReturnOK()
        {
            var title = "Test Title";
            var description = "Test Description";
            var severity = 1;
            var ipString = "192.168.82.65";
            var controller = new AlertController(_alertRepository);


            var result = await controller.AddNewAlert( title,description,severity, ipString);

            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(OkObjectResult));
        }

        [Fact]
        public async void AlertController_UpdateAlertDescription_ReturnOK()
        {
            var alertID = 1;
            var description = "Test Description";
          
            var controller = new AlertController(_alertRepository);


            var result = await controller.UpdateAlertDescription(alertID, description);

            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(OkObjectResult));
        }
    }
}
