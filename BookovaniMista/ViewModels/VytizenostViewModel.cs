using System;
using System.Collections.Generic;

namespace BookovaniMista.ViewModels
{
    public class VytizenostRadekViewModel
    {
        public string SekceNazev { get; set; } = string.Empty;
        public string MistoOznaceni { get; set; } = string.Empty;
        public int BookedDays { get; set; }
    }
}