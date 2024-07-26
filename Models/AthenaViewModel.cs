using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPV_Loader.Models
{
    public class AthenaViewModel
    {
        public IEnumerable<AthenaJob> AthenaList { get; set; }
        public AthenaJob AthenaDetails { get; set; }
    }
}