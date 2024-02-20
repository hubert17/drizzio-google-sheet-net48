using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GoogleSheetsWrapper;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.GetRequest;

namespace ASPNETWebApp48.Services
{
    public static class GsheetIntegration
    {
        public static IList<object>[] Run()
        {
            // Get the Google Spreadsheet Config Values
            var serviceAccount = "drizzio@drizzio-gsheet-integration.iam.gserviceaccount.com";
            var documentId = "1Ho9XvE7eJlI09Yh1xfH32FOyN-OQCJLbkF0SahurwH8";
            var jsonCredsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "drizzio-gsheet-integration-23a9889a1516.json");

            // In this case the json creds file is stored locally, but you can store this however you want to (Azure Key Vault, HSM, etc)
            var jsonCredsContent = System.IO.File.ReadAllText(jsonCredsPath);

            // Create a new SheetHelper class
            var sheetHelper = new SheetHelper(documentId, serviceAccount, "");
            sheetHelper.Init(jsonCredsContent);

            // Get all the rows for the first 2 columns in the spreadsheet
            var rows = sheetHelper.GetRows(new SheetRange("", 1, 1, 2),
                ValueRenderOptionEnum.FORMATTEDVALUE,
                DateTimeRenderOptionEnum.FORMATTEDSTRING);

            // Write all the values from the result set
            foreach (var row in rows)
            {
                foreach (var col in row)
                {
                    Console.Write($"{col}\t");
                }
                Console.Write("\n");
            }

            var appender = new SheetAppender(sheetHelper);

            // Appends weakly typed rows to the spreadsheeet
            appender.AppendRows(new List<List<string>>()
            {
                new List<string>(){"7/1/2022", "abc"},
                new List<string>(){"8/1/2022", "def"}
            });

            // Get all the rows for the first 2 columns in the spreadsheet
            var rows2 = sheetHelper.GetRows(new SheetRange("", 1, 1, 2),
                ValueRenderOptionEnum.FORMATTEDVALUE,
                DateTimeRenderOptionEnum.FORMATTEDSTRING);

            // Write all the values from the result set
            //foreach (var row in rows2)
            //{
            //    foreach (var col in row)
            //    {
            //        Console.Write($"{col}\t");
            //    }
            //    Console.Write("\n");
            //}

            return rows2.ToArray(); ;
        }
    }
}