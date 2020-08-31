using AgGateway.ADAPT.ApplicationDataModel.ADM;
using AgGateway.ADAPT.ApplicationDataModel.Common;
using AgGateway.ADAPT.ApplicationDataModel.LoggedData;
using AgGateway.ADAPT.ApplicationDataModel.Logistics;
using AgGateway.ADAPT.ApplicationDataModel.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace consoleApp
{
    public class SetupData
    {
        public static string DescribeLogisticsData(Catalog catalog, LoggedData loggedData)
        {
            //GFF Ids are properties of the LoggedData object
            int? growerId = loggedData.GrowerId;
            int? farmId = loggedData.FarmId;
            int? fieldId = loggedData.FieldId;

            //------------------------------
            //About ADAPT Id objects...
            //------------------------------
            //An Id in adapt is a complex type, allowing for the storage of multiple Identifiers

            //Every Id has a ReferenceId property, which is an integer scoped at the current instance
            //of the ApplicationDataModel (ADM).   These Ids are only valid withing the lifetime of that 
            //ADM object.

            //An Id object may also contain one or more permanent Ids with an associated key value to identify the type of Id.
            //See Field and Crop below for examples of mappings to the Ids created by the 20|20 monitor,
            //and Product further down for an example of the inclusion of AgX Id, which is a particular agriculture industry cross reference Id.

            //Reference Ids are often negative integers, following a convention from ISO11783, where the monitor
            //reports out data with negative ids, and the Farm Management Software prescribes data with positive ids.
            
            string fileSummary = "";
            
            //When an ADAPT object stores an "Id" integer, that integer always refers to the corresponding ReferenceId property
            if (growerId.HasValue)
            {
                Grower grower = catalog.Growers.SingleOrDefault(g => g.Id.ReferenceId == growerId.Value);
                if (grower != null)
                {
                   fileSummary = $"{String.Concat(grower.Name.Where(c => !Char.IsWhiteSpace(c)))}";
                }
            }

            if (farmId.HasValue)
            {
                Farm farm = catalog.Farms.SingleOrDefault(f => f.Id.ReferenceId == farmId.Value);
                if (farm != null)
                {
                   fileSummary = $"{fileSummary}_{String.Concat(farm.Description.Where(c => !Char.IsWhiteSpace(c)))}";
                }
            }

            if (fieldId.HasValue)
            {
                Field field = catalog.Fields.SingleOrDefault(f => f.Id.ReferenceId == fieldId.Value);
                if (field != null)
                {
                   fileSummary = $"{fileSummary}_{String.Concat(field.Description.Where(c => !Char.IsWhiteSpace(c)))}";
                }
            }

            //A Cropzone is an ADAPT contstruct to define an instance of a field for a particular growing season and particular crop.
            //While a Field can have multiple Cropzones in ADAPT for multiple crops on the same field, 
            //the Precision Planting plugin will always have a single crop per 2020 file, and thus a single cropzone.

            int? cropzoneId = loggedData.CropZoneId;
            if (cropzoneId.HasValue)
            {
                CropZone cropzone = catalog.CropZones.SingleOrDefault(c => c.Id.ReferenceId == cropzoneId.Value);
                if (cropzone != null)
                {
                    // problem when different growing season for the same field in the same folder
                    TimeScope seasonTimeScope = cropzone.TimeScopes.SingleOrDefault(t => t.DateContext == AgGateway.ADAPT.ApplicationDataModel.Common.DateContextEnum.CropSeason);
                    int growingSeason = seasonTimeScope.TimeStamp1.Value.Year;
                   fileSummary = $"{fileSummary}_{growingSeason}";
                }
            }

            Console.WriteLine($"The name of the CSV file will be {fileSummary}.csv");
            return fileSummary;
        }
    }
}
