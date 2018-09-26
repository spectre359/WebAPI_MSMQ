using WebAPI_MSMQ.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Net.Http;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;

namespace WebAPI_MSMQ.Controllers
{
    public class CustomerController : Controller
    {
        private string apibaseUrl = WebConfigurationManager.AppSettings["Forth.API.BaseUrl"];

        public ActionResult Index()
        {
            return View();
        }
        //This action uses MSMQ to get the list from the API
        public PartialViewResult CustomersList(List<CustomerViewModel> customers = null)
        {
            if (customers == null)
            {
                var responseAddress = Guid.NewGuid().ToString().Substring(0, 6);
                responseAddress = ".\\private$\\" + responseAddress;                
               
                try
                {
                    using (var responseQueue = MessageQueue.Create(responseAddress))
                    {
                        var messageModel = new Models.Message
                        {
                            URL = apibaseUrl
                        };
                        
                        using (var requestQueue = MessageQueue.Exists(".\\private$\\customersQueue") ? new MessageQueue(@".\private$\customersQueue") : MessageQueue.Create(".\\private$\\customersQueue"))
                        {
                            var message = new System.Messaging.Message();
                            var jsonMessage = Newtonsoft.Json.JsonConvert.SerializeObject(messageModel);

                            message.BodyStream = new MemoryStream(Encoding.Default.GetBytes(jsonMessage));
                            message.ResponseQueue = responseQueue;
                            message.Label = "customers";
                            requestQueue.Send(message);
                        }
                        var response = responseQueue.Receive();

                        var reader = new StreamReader(response.BodyStream);
                        var json = reader.ReadToEnd();
                        customers = JsonConvert.DeserializeObject<List<CustomerViewModel>>(json);
                        
                    }
                    if (customers.Count > 0)
                        Session["customers"] = customers;
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    }

                }
                catch (Exception ex)
                {
                    customers = new List<CustomerViewModel>();

                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                finally
                {
                    if (MessageQueue.Exists(responseAddress))
                    {
                        MessageQueue.Delete(responseAddress);
                    }
                }

            }
            return PartialView("_CustomersList", customers);
        }
        //This action connects directly to the API
        public ActionResult CustomerDetails(string id)
        {
            CustomerViewModel customer = new CustomerViewModel();
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(apibaseUrl);
                    var responseTask = client.GetAsync(String.Format("customer/{0}", id));
                    responseTask.Wait();

                    var result = responseTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        IEnumerable<OrderViewModel> orders = null;
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();
                        customer = JsonConvert.DeserializeObject<CustomerViewModel>(readTask.Result);
                        if (customer != null)
                        {
                            responseTask = client.GetAsync(String.Format("customer/{0}/orders", id));
                            responseTask.Wait();

                            result = responseTask.Result;
                            if (result.IsSuccessStatusCode)
                            {
                                readTask = result.Content.ReadAsStringAsync();
                                readTask.Wait();
                                orders = JsonConvert.DeserializeObject<IEnumerable<OrderViewModel>>(readTask.Result);
                                customer.Orders = orders;
                            }
                        }
                        else
                        {
                            customer = new CustomerViewModel();
                            ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
                    }
                }
            }
            catch (Exception ex)
            {
                customer = new CustomerViewModel();
                ModelState.AddModelError(string.Empty, ex.Message);
            }

            return View(customer);
        }

        [HttpPost]
        public ActionResult Search(FormCollection form)
        {
            List<CustomerViewModel> clients = new List<CustomerViewModel>();
            var searchCriteria = form["searchfield"].ToString();
            if (Session["customers"] != null)
            {
                if (String.IsNullOrEmpty(searchCriteria))
                    clients = Session["customers"] as List<CustomerViewModel>;
                else
                    clients = (Session["customers"] as List<CustomerViewModel>).Where(c => c.CompanyName.ToLower().Contains(searchCriteria.ToLower())).ToList();
            }

            return CustomersList(clients);
        }
    }
}