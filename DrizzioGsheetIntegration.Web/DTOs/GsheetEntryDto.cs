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

        public int QualCallCount { get; set; } // ==> Triage

        public int SalesCallCount { get; set; } // qualified calls"

        public int QualCallShowup { get; set; }

        public int SalesCallShowup { get; set; }
    }
}
