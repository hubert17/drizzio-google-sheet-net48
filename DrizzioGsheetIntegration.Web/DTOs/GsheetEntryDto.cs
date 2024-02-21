using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrizzioGsheetIntegration.Shared.DTOs
{
    public class GsheetEntryDto
    {
        public DateTime DateEntered { get; set; }
        public int QualCallCount { get; set; }
        public int SalesCallCount { get; set; }
    }
}
