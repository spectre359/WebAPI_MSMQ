using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAPI_MSMQ.Controllers;
using WebAPI_MSMQ.Models;

namespace WebAPI_MSMQ.Tests.Controllers
{
    [TestClass]
    public class CustomerControllerTest
    {    
        [TestMethod]
        public void CustomersList_ShouldReturnCustomersListPartialViewAndModelIsOfCorrectType()
        {
            // Arrange
            CustomerController controller = new CustomerController();

            // Act
            List<CustomerViewModel> customers = new List<CustomerViewModel>()
            {
                new CustomerViewModel() {CompanyName="WebAPI_MSMQ", OrdersCount=50 },
                new CustomerViewModel() {CompanyName="Microsoft", OrdersCount=22 }
            };

            PartialViewResult result = controller.CustomersList(customers) as PartialViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("_CustomersList", result.ViewName);
            Assert.IsInstanceOfType(result.Model, typeof(IEnumerable<CustomerViewModel>));
        }
    }
}
