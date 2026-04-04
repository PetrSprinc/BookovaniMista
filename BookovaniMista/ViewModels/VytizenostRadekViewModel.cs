using System;
using System.Collections.Generic;

namespace BookovaniMista.ViewModels
{
    public class VytizenostViewModel
    {
        public DateTime Od { get; set; }
        public DateTime Do { get; set; }
        public List<VytizenostRadekViewModel> Rows { get; set; } = new();
    }
}