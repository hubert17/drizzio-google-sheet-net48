using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DrizzioGsheetIntegration.Shared.DTOs;
using DrizzioGsheetIntegration.Shared;
using GoogleSheetsWrapper;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.GetRequest;
using DrizzioGsheetIntegration.Web.Models.CMmodels;
using Google.Apis.Sheets.v4.Data;

namespace ASPNETWebApp48.Services
{
    public static class GsheetIntegration
    {
        public static List<GsheetEntryDto> GetData(int month, int year)
        {
            // qual call steps: 77daea43-22bd-4b40-acec-391f7503d412 | 429e928c-4008-48ec-be5b-c1985b978b4a | cc652721-c936-4726-b4a3-ee23cdb2d970
            // Sales call steps: 0569c71d-aee7-4867-8e5b-0e54ee9b0463 | a8e6d521-fe16-4e8a-ab24-f1523af2b2f5 | 372560b1-e8db-46d0-8e14-b53da6c48272 | d09c90ad-c8b6-4a0e-867f-c0c3921e519b

            var qualCallIds = new List<Guid>
            {
                new Guid("77daea43-22bd-4b40-acec-391f7503d412"),
                new Guid("429e928c-4008-48ec-be5b-c1985b978b4a"),
                new Guid("cc652721-c936-4726-b4a3-ee23cdb2d970")
            };

            var salesCallIds = new List<Guid>
            {
                new Guid("0569c71d-aee7-4867-8e5b-0e54ee9b0463"),
                new Guid("a8e6d521-fe16-4e8a-ab24-f1523af2b2f5"),
                new Guid("372560b1-e8db-46d0-8e14-b53da6c48272"),
                new Guid("d09c90ad-c8b6-4a0e-867f-c0c3921e519b")
            };

            var _db = new CMContext();

            var contactIds = (from cs in _db.cmContactSteps
                              join en in _db.cmEntityDatas on cs.ContactId equals en.ContactID
                              where en.Data.StartsWith("ig_ads")
                                && en.DefinitionId.ToString() == "3BA42F3C-9503-4B5C-808D-09C6EB262D86"
                                && (qualCallIds.Contains(cs.StepId) || salesCallIds.Contains(cs.StepId))
                              select cs.ContactId).Distinct().ToList();

            var data = from cs in _db.cmContactSteps
                       where contactIds.Contains(cs.ContactId)
                             && (cs.DateEntered.Month == month && cs.DateEntered.Year == year)
                       group cs by System.Data.Entity.DbFunctions.TruncateTime(cs.DateEntered) into g
                       select new GsheetEntryDto
                       {
                           DateEntered = g.Key,
                           QualCallCount = g.Count(cs => qualCallIds.Contains(cs.StepId)),
                           SalesCallCount = g.Count(cs => salesCallIds.Contains(cs.StepId)),
                           QualCallShowedUp = (from cd in _db.cmDialSessionCalls
                                               where g.Select(gs => gs.ContactId).Contains(cd.ContactID)
                                                     && cd.CallGroup == "Qualification Call"
                                                     && cd.LiveAnswer == true
                                                     && cd.Completed == true
                                               select cd.ContactID).Distinct().Count(),
                           SalesCallShowedUp = (from cd in _db.cmDialSessionCalls
                                                where g.Select(gs => gs.ContactId).Contains(cd.ContactID)
                                                      && cd.CallGroup == "Sales Call"
                                                      && cd.LiveAnswer == true
                                                      && cd.Completed == true
                                                select cd.ContactID).Distinct().Count(),
                           UnitSold = (from co in _db.cmContactOrders
                                       where g.Select(gs => gs.ContactId).Distinct().Contains(co.ContactID)
                                       select co).Count(),
                           Cost = (from co in _db.cmContactOrders
                                   where g.Select(gs => gs.ContactId).Distinct().Contains(co.ContactID)
                                   select co.Amount).Sum(),
                           Revenue = (from co in _db.cmContactOrders
                                      join fo in _db.FulfillmentOrders
                                      on co.Id equals fo.OrderID
                                      where g.Select(gs => gs.ContactId).Distinct().Contains(co.ContactID)
                                            && fo.DateCompleted != null
                                      select fo.OrderSubtotal).Sum()
                       };



            var gsheetEntries = data.OrderBy(x => x.DateEntered).ToList();
            SaveToGoogleSheet2(gsheetEntries);

            return gsheetEntries;
        }

        public static void SaveToGoogleSheet(List<GsheetEntryDto> gsheetEntries)
        {
            // Get the Google Spreadsheet Config Values
            var serviceAccount = "drizzio@drizzio-gsheet-integration.iam.gserviceaccount.com";
            var documentId = "1pfTfdZaJQXUQLBnT6OidABDBN8zAwky31JNl80x1aC8";
            var jsonCredsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "drizzio-gsheet-integration-23a9889a1516.json");

            // In this case the json creds file is stored locally, but you can store this however you want to (Azure Key Vault, HSM, etc)
            var jsonCredsContent = System.IO.File.ReadAllText(jsonCredsPath);

            // Create a new SheetHelper class
            var sheetHelper = new SheetHelper(documentId, serviceAccount, "");
            sheetHelper.Init(jsonCredsContent);

           // Get all the rows for the first 2 columns in the spreadsheet

           var rows = sheetHelper.GetRows(new SheetRange("February", 1, 1, 2),
               ValueRenderOptionEnum.FORMATTEDVALUE,
               DateTimeRenderOptionEnum.FORMATTEDSTRING);

            var _rowsToAppend = new List<List<string>>();

            gsheetEntries.ForEach(entry =>
            {
                List<string> _row = new List<string>
                {
                    entry.DateEntered.ToString(),  
                    null,
                    null,
                    entry.QualCallCount.ToString(),
                    entry.SalesCallCount.ToString() 
                };
                _rowsToAppend.Add(_row);
            });

            var appender = new SheetAppender(sheetHelper);

            // Appends weakly typed rows to the spreadsheeet
            appender.AppendRows(_rowsToAppend);

            // Get all the rows for the first 2 columns in the spreadsheet
            //var rows2 = sheetHelper.GetRows(new SheetRange("", 1, 1, 2),
            //    ValueRenderOptionEnum.FORMATTEDVALUE,
            //    DateTimeRenderOptionEnum.FORMATTEDSTRING);

            // Write all the values from the result set
            //foreach (var row in rows2)
            //{
            //    foreach (var col in row)
            //    {
            //        Console.Write($"{col}\t");
            //    }
            //    Console.Write("\n");
            //}

            //return rows2.ToArray(); ;
        }

        public static void SaveToGoogleSheet2(List<GsheetEntryDto> gsheetEntries)
        {
            // Get the Google Spreadsheet Config Values
            var serviceAccount = "drizzio@drizzio-gsheet-integration.iam.gserviceaccount.com";
            var documentId = "1pfTfdZaJQXUQLBnT6OidABDBN8zAwky31JNl80x1aC8";
            var jsonCredsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "drizzio-gsheet-integration-23a9889a1516.json");

            // In this case the json creds file is stored locally, but you can store this however you want to (Azure Key Vault, HSM, etc)
            var jsonCredsContent = System.IO.File.ReadAllText(jsonCredsPath);

            // Create a new SheetHelper class
            var sheetHelper = new SheetHelper(documentId, serviceAccount, "test_February Spanish");
            sheetHelper.Init(jsonCredsContent);

            // Get all the rows for the first 2 columns in the spreadsheet
            //var rows = sheetHelper.GetRows(new SheetRange("February Spanish", 1, 3, 40, 8)).ToList();


            gsheetEntries.ForEach(row =>
            {
                var gheetRow = row.DateEntered.Value.Day + 2;
                UpdateCell(sheetHelper, $"B{gheetRow}", row.DateEntered.ToString());
                UpdateCell(sheetHelper, $"X{gheetRow}", row.QualCallCount.ToString());
                UpdateCell(sheetHelper, $"AD{gheetRow}", row.QualCallShowedUp.ToString());
                UpdateCell(sheetHelper, $"AH{gheetRow}", row.SalesCallCount.ToString());
                UpdateCell(sheetHelper, $"AF{gheetRow}", row.SalesCallShowedUp.ToString());
                UpdateCell(sheetHelper, $"AK{gheetRow}", row.UnitSold.ToString());
                UpdateCell(sheetHelper, $"AN{gheetRow}", row.Revenue.ToString());
            });
        }

        static void UpdateCell(SheetHelper sheetHelper, string cell, string value)
        {
            var updates = new List<BatchUpdateRequestObject>();
            updates.Add(new BatchUpdateRequestObject()
            {
                Range = new SheetRange($"{cell}:{cell}"),
                Data = new CellData()
                {
                    UserEnteredValue = new ExtendedValue()
                    {
                        StringValue = value
                    }
                }
            });

            // Note setting the field mask to "*" tells the API to save all property values, even if they are null / blank
            sheetHelper.BatchUpdate(updates, "*");
        }

        static DateTime ToCSharpDate(double serialNumber)
        {
            // The base date for Google Sheets is December 30, 1899
            DateTime baseDate = new DateTime(1899, 12, 30);

            // Add the number of days (whole part) to the base date
            DateTime result = baseDate.AddDays(serialNumber);

            // Google Sheets also supports fractions of a day, which we can convert to hours, minutes, seconds, etc.
            double fraction = serialNumber - Math.Floor(serialNumber);
            result = result.AddHours(24 * fraction);

            return result;
        }

    }
}