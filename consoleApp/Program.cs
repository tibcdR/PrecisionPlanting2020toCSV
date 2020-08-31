using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using System.Text;

//to get hybrid names
using AgGateway.ADAPT.ApplicationDataModel.Equipment;
using AgGateway.ADAPT.ApplicationDataModel.Products;


namespace consoleApp
{
    static class Program
    {
        //For a graphical user interface, clone https://github.com/ADAPT/ADAPT-Visualizer and follow the usage instructions in http://2020.ag/developers.html 
        //This example code provides examples specific to data populated into the ADAPT model by the 2020 Plugin.

        static void Main()
        {
                       
            Console.WriteLine();

            // set arguments below
                //Arg 0 - path to data
                //-5: 5hz data frequency
                //-1: 1hz data frequency (default)
                //-all: include all optional data (default is none)
            string[] args = new string[] {@".\consoleApp\sampleData", "-5", "-all"}; //Console.ReadLine().Split(' ');
            string dirToWrite = @"XXX file_path where to write csv files";

            Console.WriteLine($"The arguments are {string.Join(" ", args)}. Change in Program.cs line 27 if needed");
            Console.WriteLine($"Data will be written to {dirToWrite}");
            string dataPath = args.Any() ? args[0] : null;

            while (!Directory.Exists(dataPath))
            {
                Console.WriteLine("Input is not a valid directory path.   Try again.");
                dataPath = Console.ReadLine();
            }

            Console.WriteLine("Looking for data...");

            //Instantiate the 2020 plugin
            PrecisionPlanting.ADAPT._2020.Plugin _2020Plugin = new PrecisionPlanting.ADAPT._2020.Plugin();

            //Load any properties to customize the data import
            Properties pluginProperties = GetPluginProperties(args);

            //Import data from the given path.
            IList<ApplicationDataModel> admObjects = _2020Plugin.Import(dataPath, pluginProperties);

            if (admObjects != null && admObjects.Any())
            {
                //A 2020 Plugin import will always contain 1 ApplicationDataModel (ADM) object.    All data in the import path is included within this object.
                //The ADAPT Framework Plugin Import method returns a list of ADM objects to support other industry data types (e.g., ISOXML) 
                //where there is a concept of multiple wholly-contained datasets in a given file path.
                ApplicationDataModel adm = admObjects.Single();

                //The Catalog contains definition data for this import
                Catalog catalog = adm.Catalog;

                //The LoggedDataobject corresponds to a single 2020 file and defines one or more operations on a particular field.
                int loggedDataCount = adm.Documents.LoggedData.Count();
                if (loggedDataCount > 0)
                {
                    //loop through files of the input data folder
                    foreach (LoggedData loggedata in adm.Documents.LoggedData)
                    {
                        string CSVfilename = SetupData.DescribeLogisticsData(catalog, loggedata);

                        //loop through 20|20 file and create one csv per opdata
                        foreach (OperationData opdata in loggedata.OperationData){
                            
                            //Make a dictionary of product names
                            // Dictionary<int, string> productNames = new Dictionary<int, string>();
                            // foreach (int productID in opdata.ProductIds)
                            // {

                            //     List<DeviceElementUse> productDeviceElementUses = new List<DeviceElementUse>();
                            //     Product product = catalog.Products.First(p => p.Id.ReferenceId == productID);
                            //     Console.WriteLine(productID);
                            //     productNames.Add(productID, product.Description);
                            // }
                            // Console.WriteLine($"The following varieties are planted at various points in the field: {string.Join(", ", productNames.Values)}");

                            List<SpatialRecord> spatialRecords = new List<SpatialRecord>();
                            if (opdata.GetSpatialRecords != null)
                            {
                                spatialRecords = opdata.GetSpatialRecords().ToList(); //Iterate the records once here for multiple consumers below
                            }

                            DataTable data = new DataTable();
                            ExportSpatialData exporter = new ExportSpatialData();
                            data = exporter.ProcessOperationData(opdata, spatialRecords);

                            string filepath = $"{dirToWrite}{CSVfilename}.csv";

                            int c = 1;
                            while (File.Exists(filepath))
                            {
                                filepath = $"{dirToWrite}{CSVfilename}_{c}.csv";
                                c++;
                            }
                            Console.WriteLine(filepath);
                            data.ToCsv(filepath);
                        }
                    }
                }

                Console.WriteLine("Done");

                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("No data found");
                Console.ReadLine();
            }
        }

        private static Properties GetPluginProperties(string[] args)
        {
            //The plugin properties provide a means to limit the amount of data returned, improving performance by eliminated data or detail not required.
            //By default, the plugin downsamples raw 5Hz machine data to 1Hz.   If you wish to obtain the full 5Hz data, pass the DataFrequency parameter = 5
            //Similarly, you can opt to include optional certain sensors (e.g., Downforce = true) to increase the amount of data returned
            //See http://2020.ag/developers.html for a list of properties and values.
            Properties pluginProperties = null;
            if (args.Any() && args.Count() > 1)
            {
                pluginProperties = new Properties();
                if (args.Contains("-5"))
                {
                    pluginProperties.SetProperty("DataFrequency", "5");
                }
                else
                {
                    pluginProperties.SetProperty("DataFrequency", "1");
                }

                if (args.Contains("-all"))
                {
                    pluginProperties.SetProperty("Downforce", "true");
                    pluginProperties.SetProperty("SeedingQuality", "true");
                    pluginProperties.SetProperty("SoilSensing", "true");
                    pluginProperties.SetProperty("Insecticide", "true");
                    pluginProperties.SetProperty("LiquidApplication", "true");
                    pluginProperties.SetProperty("RowUnitDepthControl", "true");
                    pluginProperties.SetProperty("RowUnitClosingSystem", "true");
                    pluginProperties.SetProperty("RowTotals", "true");
                }
               
            }
            return pluginProperties;
        }

        private static void ToCsv(this DataTable dataTable, string filePath, string separator = ";") 
        {
            StringBuilder fileContent = new StringBuilder();

            foreach (var col in dataTable.Columns) 
            {
                fileContent.Append(col.ToString() + separator);
            }

            fileContent.Replace(separator, System.Environment.NewLine, fileContent.Length - 1, 1);

            foreach (DataRow dr in dataTable.Rows) 
            {
                foreach (var column in dr.ItemArray) 
                {
                    fileContent.Append("\"" + column.ToString() + $"\"{separator}");
                }

                fileContent.Replace(separator, System.Environment.NewLine, fileContent.Length - 1, 1);
            }

            System.IO.File.WriteAllText(filePath, fileContent.ToString(), Encoding.UTF8);
        }
    }
}