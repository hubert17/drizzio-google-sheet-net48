using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrizzioGsheetIntegration.Shared.DTOs
{
    public class GsheetEntryDto
    {
        public DateTime? DateEntered { get; set; } // ==> Date

        public int QualCallCount { get; set; } // ==> Triage Calls

        public int SalesCallCount { get; set; } // qualified calls

        public int QualCallShowedUp { get; set; } // showed Up Triage

        public int SalesCallShowedUp { get; set; } // "Showed Up Sales Call

        public int? UnitSold { get; set; }      

        public decimal? Cost { get; set; } // Which Cost column are you referring to here?

        public decimal? Revenue { get; set; }
    }
}
