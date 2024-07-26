using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Net;

namespace SPV_Loader.Controllers
{
    public class ImportOrderController : Controller
    {
        private readonly string _connectionString = "Data Source=CM-APP-SVR\\SQLEXPRESS;Initial Catalog=SpvLoader;Integrated Security=true";

        [HttpPost]
        public ActionResult ImportOrder(HttpPostedFileBase postedFile)
        {
            if (postedFile == null || postedFile.ContentLength == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No file uploaded.");
            }

            try
            {
                // Save the uploaded file to a temporary path
                string tempFilePath = Server.MapPath("~/App_Data/temp.xls");
                postedFile.SaveAs(tempFilePath);

                // Load the XML document
                XDocument xmlDoc = XDocument.Load(tempFilePath);

                // Determine the structure and count of elements in the XML
                int numOfJobs = xmlDoc.Root.Elements().Count();
                int numOfFieldsInJob = xmlDoc.Root.Elements().ElementAt(0).Elements().ElementAt(0).Elements().Count();

                // Create an array to store job information
                Order[] jobsArray = new Order[numOfJobs];

                for (int i = 0; i < numOfJobs; i++)
                {
                    // Create and fill string array with values from XML file
                    string[] values = new string[numOfFieldsInJob];

                    for (int j = 0; j < numOfFieldsInJob; j++)
                    {
                        if (xmlDoc.Root.Elements().ElementAt(i).Elements().ElementAt(0).Elements().ElementAt(j).Elements().ElementAt(0).Value == "")
                        {
                            values[j] = "N/A";
                        }
                        else
                        {
                            values[j] = xmlDoc.Root.Elements().ElementAt(i).Elements().ElementAt(0).Elements().ElementAt(j).Elements().ElementAt(0).Value;
                        }
                    }

                    // Create new order and place it in array of orders
                    jobsArray[i] = new Order((i + 1).ToString(), values);
                }

                // Convert jobsArray to DataTable
                DataTable jobs = new DataTable("Jobs");
                jobs.Columns.Add("Id");
                jobs.Columns.Add("JobNumber");
                jobs.Columns.Add("DueDate");
                jobs.Columns.Add("PurchaseOrderNumber");
                jobs.Columns.Add("PurchaseOrderLine");
                jobs.Columns.Add("SalesOrderNumber");
                jobs.Columns.Add("CustomerAccountCode");
                jobs.Columns.Add("JobQuantity");
                jobs.Columns.Add("AscmOrderId");
                jobs.Columns.Add("EndCustomer");
                jobs.Columns.Add("ActivationSystem");
                jobs.Columns.Add("ProductType");
                jobs.Columns.Add("ErpMaterialCode");
                jobs.Columns.Add("IntegratorPartId");
                jobs.Columns.Add("IntegratorID");
                jobs.Columns.Add("ActivationType");
                jobs.Columns.Add("PartNumberSku");
                jobs.Columns.Add("RetailBarcode");
                jobs.Columns.Add("RetailBarcodeType");
                jobs.Columns.Add("OCR");
                jobs.Columns.Add("Channel");

                foreach (var order in jobsArray)
                {
                    DataRow row = jobs.NewRow();
                    row["Id"] = order.Id;
                    for (int i = 0; i < order.Values.Length; i++)
                    {
                        row[i + 1] = order.Values[i];
                    }
                    jobs.Rows.Add(row);
                }

                // Delete existing data from the database tables
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    SqlCommand cmd1 = new SqlCommand("DELETE FROM AthenaJobs", con);
                    SqlCommand cmd2 = new SqlCommand("DELETE FROM ExportAthena", con);
                    cmd1.ExecuteNonQuery();
                    cmd2.ExecuteNonQuery();
                    con.Close();
                }

                // Insert new data into the database
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    int varID = 0;

                    foreach (DataRow row in jobs.Rows)
                    {
                        varID++;
                        using (SqlCommand cmd = new SqlCommand(@"INSERT INTO AthenaJobs 
                            (Id, JobNumber, DueDate, PurchaseOrderNumber, PurchaseOrderLine, SalesOrderNumber, CustomerAccountCode, JobQuantity, AscmOrderId, EndCustomer, ActivationSystem, ProductType, ErpMaterialCode, IntegratorPartId, IntegratorID, ActivationType, PartNumberSku, RetailBarcode, RetailBarcodeType, Channel)
                            VALUES 
                            (@Id, @JobNumber, @DueDate, @PurchaseOrderNumber, @PurchaseOrderLine, @SalesOrderNumber, @CustomerAccountCode, @JobQuantity, @AscmOrderId, @EndCustomer, @ActivationSystem, @ProductType, @ErpMaterialCode, @IntegratorPartId, @IntegratorID, @ActivationType, @PartNumberSku, @RetailBarcode, @RetailBarcodeType, @Channel)", con))
                        {
                            cmd.Parameters.AddWithValue("@Id", varID);
                            cmd.Parameters.AddWithValue("@JobNumber", row["JobNumber"].ToString());
                            cmd.Parameters.AddWithValue("@DueDate", row["DueDate"].ToString());
                            cmd.Parameters.AddWithValue("@PurchaseOrderNumber", row["PurchaseOrderNumber"].ToString());
                            cmd.Parameters.AddWithValue("@PurchaseOrderLine", row["PurchaseOrderLine"].ToString());
                            cmd.Parameters.AddWithValue("@SalesOrderNumber", row["SalesOrderNumber"].ToString());
                            cmd.Parameters.AddWithValue("@CustomerAccountCode", row["CustomerAccountCode"].ToString());
                            cmd.Parameters.AddWithValue("@JobQuantity", row["JobQuantity"].ToString());
                            cmd.Parameters.AddWithValue("@AscmOrderId", row["AscmOrderId"].ToString());
                            cmd.Parameters.AddWithValue("@EndCustomer", row["EndCustomer"].ToString());
                            cmd.Parameters.AddWithValue("@ActivationSystem", row["ActivationSystem"].ToString());
                            cmd.Parameters.AddWithValue("@ProductType", row["ProductType"].ToString());
                            cmd.Parameters.AddWithValue("@ErpMaterialCode", row["ErpMaterialCode"].ToString());
                            cmd.Parameters.AddWithValue("@IntegratorPartId", row["IntegratorPartId"].ToString());
                            cmd.Parameters.AddWithValue("@IntegratorID", row["IntegratorID"].ToString());
                            cmd.Parameters.AddWithValue("@ActivationType", row["ActivationType"].ToString());
                            cmd.Parameters.AddWithValue("@PartNumberSku", row["PartNumberSku"].ToString());
                            cmd.Parameters.AddWithValue("@RetailBarcode", row["RetailBarcode"].ToString());
                            cmd.Parameters.AddWithValue("@RetailBarcodeType", row["RetailBarcodeType"].ToString());
                            cmd.Parameters.AddWithValue("@Channel", row["Channel"].ToString());
                            cmd.ExecuteNonQuery();
                        }
                    }
                    con.Close();
                }

                // Fetch the newly inserted data to update the AthenaJobs list in the AthenaController
                using (SpvLoaderEntities context = new SpvLoaderEntities())
                {
                    var athenaJobs = context.AthenaJobs.ToList();
                    var athenaController = DependencyResolver.Current.GetService<AthenaController>();
                    athenaController.SetAthenaJobs(athenaJobs);
                }

                return RedirectToAction("Index", "Athena");
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public class Order
        {
            public string Id { get; set; }
            public string[] Values { get; set; }

            public Order() { }

            public Order(string id, string[] values)
            {
                Id = id;
                Values = values;
            }
        }
    }
}
