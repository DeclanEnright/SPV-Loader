using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using SPV_Loader.Models;

namespace SPV_Loader.Controllers
{
    public class AthenaController : Controller
    {
        private readonly string _connectionString = "Data Source=CM-APP-SVR\\SQLEXPRESS;Initial Catalog=SpvLoader;Integrated Security=true";

        // Track the current index
        private static int currentIndex = 0;
        private static List<AthenaJob> athenaJobs = null;

        public ActionResult Index()
        {
            if (athenaJobs == null)
            {
                athenaJobs = new List<AthenaJob>(); // Initialize as empty list
            }

            AthenaJob details = null;
            if (currentIndex < athenaJobs.Count)
            {
                details = athenaJobs[currentIndex]; // Get the details object for the current index
            }

            var viewModel = new AthenaViewModel
            {
                AthenaList = athenaJobs,
                AthenaDetails = details
            };

            if (currentIndex >= athenaJobs.Count && athenaJobs.Any())
            {
                currentIndex = 0; // Reset index if it exceeds the list
                athenaJobs = new List<AthenaJob>(); // Clear the list
                ViewBag.AllItemsProcessed = true;
            }
            else
            {
                ViewBag.AllItemsProcessed = false;
            }

            return View(viewModel);
        }

        public List<AthenaJob> GetItems()
        {
            List<AthenaJob> jobs = new List<AthenaJob>();
            try
            {
                using (SpvLoaderEntities context = new SpvLoaderEntities())
                {
                    jobs = context.AthenaJobs.ToList();
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                TempData["errorMessage"] = ex.ToString();
            }
            return jobs;
        }

        public AthenaJob GetDetails(int id)
        {
            AthenaJob detail = null;
            try
            {
                using (SpvLoaderEntities context = new SpvLoaderEntities())
                {
                    detail = context.AthenaJobs.FirstOrDefault(d => d.Id == id);
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                TempData["errorMessage"] = ex.ToString();
            }
            return detail;
        }

        [HttpPost]
        public ActionResult Save(AthenaViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (SpvLoaderEntities context = new SpvLoaderEntities())
                    {
                        var jobToUpdate = context.AthenaJobs.FirstOrDefault(j => j.Id == model.AthenaDetails.Id);

                        if (jobToUpdate != null)
                        {
                            // Update the job details with the values from the model
                            jobToUpdate.JobNumber = model.AthenaDetails.JobNumber;
                            jobToUpdate.DueDate = model.AthenaDetails.DueDate;
                            jobToUpdate.PurchaseOrderNumber = model.AthenaDetails.PurchaseOrderNumber;
                            jobToUpdate.PurchaseOrderLine = model.AthenaDetails.PurchaseOrderLine;
                            jobToUpdate.SalesOrderNumber = model.AthenaDetails.SalesOrderNumber;
                            jobToUpdate.CustomerAccountCode = model.AthenaDetails.CustomerAccountCode;
                            jobToUpdate.JobQuantity = model.AthenaDetails.JobQuantity;
                            jobToUpdate.AscmOrderId = model.AthenaDetails.AscmOrderId;
                            jobToUpdate.EndCustomer = model.AthenaDetails.EndCustomer;
                            jobToUpdate.ActivationSystem = model.AthenaDetails.ActivationSystem;
                            jobToUpdate.ProductType = model.AthenaDetails.ProductType;
                            jobToUpdate.ErpMaterialCode = model.AthenaDetails.ErpMaterialCode;
                            jobToUpdate.IntegratorPartID = model.AthenaDetails.IntegratorPartID;
                            jobToUpdate.IntegratorID = model.AthenaDetails.IntegratorID;
                            jobToUpdate.ActivationType = model.AthenaDetails.ActivationType;
                            jobToUpdate.PartNumberSku = model.AthenaDetails.PartNumberSku;
                            jobToUpdate.RetailBarcode = model.AthenaDetails.RetailBarcode;
                            jobToUpdate.RetailBarcodeType = model.AthenaDetails.RetailBarcodeType;
                            jobToUpdate.Channel = model.AthenaDetails.Channel;

                            // Save changes to the database
                            context.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the error or handle it as needed
                    TempData["errorMessage"] = ex.ToString();
                }
            }

            // Move to the next item in the list
            currentIndex++;
            return RedirectToAction("Index");
        }

        // Method to set the athenaJobs list
        public void SetAthenaJobs(List<AthenaJob> jobs)
        {
            athenaJobs = jobs;
            currentIndex = 0; // Reset the index
        }

        // Download action for the processed list
        [HttpPost]
        public ActionResult Download()
        {
           

            return new EmptyResult();
        }
    }
}

