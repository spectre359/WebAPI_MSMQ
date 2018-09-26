using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebAPI_MSMQ.API.Controller;
using System.Collections.Generic;
using WebAPI_MSMQ.Models;
using System.Web.Http;
using System.Web.Http.Results;

namespace WebAPI_MSMQ.API.Tests
{
    [TestClass]
    public class CustomersTest
    {
        [TestMethod]
        public void GetAllCustomers_ShouldReturnOKStatus()
        {
            var controller = new CustomersController();
            IHttpActionResult result = controller.GetAllCustomers();
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<IEnumerable<CustomerViewModel>>));
        }

        [TestMethod]
        public void GetCustomerByID_ShouldReturnCorrectCustomer()
        {
            var controller = new CustomersController();
            var result = controller.GetCustomerById("ALFKI") as OkNegotiatedContentResult<CustomerViewModel>;
            Assert.IsNotNull(result);
            Assert.AreEqual("Alfreds Futterkiste", result.Content.CompanyName);
        }

        [TestMethod]
        public void GetCustomerByID_ShouldNotFindCustomer()
        {
            var controller = new CustomersController();
            var result = controller.GetCustomerById("Phil");
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public void GetOrdersByCustomer_ShouldReturnOKStatus()
        {
            var controller = new CustomersController();
            IHttpActionResult result = controller.GetOrdersByCustomer("ALFKI");
            Assert.IsInstanceOfType(result, typeof(OkNegotiatedContentResult<IEnumerable<OrderViewModel>>));
        }
        
    }
}
