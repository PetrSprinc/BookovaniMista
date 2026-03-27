using System;
using System.Collections.Generic;

namespace BookovaniMista.ViewModels
{
    public class VytizenostRadek
    {
        public string SekceNazev { get; set; } = string.Empty;
        public string MistoOznaceni { get; set; } = string.Empty;
        public int BookedDays { get; set; }
    }

    public class VytizenostViewModel
    {
        public DateTime Od { get; set; }
        public DateTime Do { get; set; }
        public List<VytizenostRadek> Rows { get; set; } = new();
    }
}