using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPI_MSMQ.Models;
using System.Web.Http.Cors;

namespace WebAPI_MSMQ.API.Controller
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api")]
    public class CustomersController : ApiController
    {
        [Route("customers")]
        public IHttpActionResult GetAllCustomers()
        {
            IEnumerable<CustomerViewModel> customers = null;

            using (var db = new NorthwindEntities())
            {
                customers = db.Customers
                            .Select(c => new CustomerViewModel()
                            {
                                CustomerID = c.CustomerID,
                                CompanyName = c.CompanyName,
                                OrdersCount = c.Orders.Count
                            }).ToList();
            }

            if (customers.Count() == 0)
            {
                return NotFound();
            }

            return Ok(customers);
        }

        [Route("customer/{id}")]
        public IHttpActionResult GetCustomerById(string id)
        {
            CustomerViewModel customer = null;

            using (var db = new NorthwindEntities())
            {
                customer = db.Customers
                    .Where(c => c.CustomerID.Equals(id))
                    .Select(c => new CustomerViewModel()
                    {
                        CustomerID = c.CustomerID,
                        CompanyName = c.CompanyName,
                        ContactName = c.ContactName,
                        ContactTitle = c.ContactTitle,
                        Address = c.Address,
                        City = c.City,
                        Region = c.Region,
                        PostalCode = c.PostalCode,
                        Country = c.Country,
                        Phone = c.Phone,
                        Fax = c.Fax
                    }).FirstOrDefault();
            }

            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        [Route("customer/{id}/orders")]
        public IHttpActionResult GetOrdersByCustomer(string id)
        {
            IEnumerable<OrderViewModel> orders = null;

            using (var db = new NorthwindEntities())
            {
                orders = db.Orders.Where(o => o.CustomerID == id).Select(o => new OrderViewModel()
                {
                    OrderID = o.OrderID,
                    ProductQuantity = o.Order_Details.Sum(od => od.Quantity),
                    TotalPrice = Math.Round(o.Order_Details.Sum(od => od.Quantity * od.UnitPrice), 2),
                    InformationMessage = o.Order_Details.Any(od => od.Product.Discontinued) ? "One or more of the products in the order has been discontinued. There might be a problem with order completion." :
                        (o.Order_Details.Any(od => od.Product.UnitsOnOrder > od.Product.UnitsInStock) ? "There are more products requested in the order, than those available." : String.Empty)
                }
                ).ToList();
            }

            if (orders == null)
            {
                return NotFound();
            }

            return Ok(orders);
        }
    }
}
