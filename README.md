# PrecisionPlanting2020 to CSV

This console app has been designed to simply extract data from Precision Planting 20|20 GEN 3 files and write it in a CSV file. I used code from PrecisionPlanting2020 ADAPT plugin examples  (https://github.com/PrecisionPlanting/ADAPT-2020-examples.git) and code from ADAPT Visualizer (https://github.com/ADAPT/ADAPT-Visualizer.git).
The input is a .2020 file and the output is one csv per OperationData.

The arguments are : 
  * the input data folder with .2020 files that you want to extract ; 
  * the output folder, where the csv will be written ;
  * the data frequency : 5Hz or 1Hz ;
  * what type of data to extract : -all to extract every type of data (for other type see https://2020.ag/adapt-download/)
Change the arguments line 27 and 28.

Sample data to test the app is available in sampleData directory (data with one hybrid per field and with two hybrids per field) The data are from Precision Planting (https://2020.ag/adapt-usage/)

The app works in 4 steps : 
  * the .2020 files are imported with Precision.Planting.ADAPT.2020 plugin and implemented in Application Data Model
  * for each file (loggedData) a filename is generated with the following scheme : GrowerName_FarmName_FieldName_GrowingSeason.csv so for the sample 2020 data included : "Tremont_R&D_vDrive1hybrid_2018.csv"
  * the code loops through OperationData in LoggedData : for each OperationData the data are imported in a DataTable
  * the DataTable is then written in a .csv file (if the file already exists an increment is appended to the filename)

The CSV file is generated with semicolon separator (";"), the decimal seprator is ".". If you want comma separated value, you can change the separator default argument of ToCsv function (Program.cs) and set it to ",".

# To run the console app
dotnet run -p .\consoleApp\consoleApp.csproj
