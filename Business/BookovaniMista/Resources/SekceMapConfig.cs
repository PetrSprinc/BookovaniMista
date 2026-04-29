namespace Business.BookovaniMista.Resources
{
    public class SekceMapConfig
    {
        public int Id { get; set; }
        public string AnchorId { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int LabelX { get; set; }
        public int LabelY { get; set; }
        public string Nazev { get; set; } = string.Empty;
    }

    public static class MapConfiguration
    {
        // ViewBox parametry
        public const int SvgViewBoxWidth = 1000;
        public const int SvgViewBoxHeight = 500;
        public const string SvgViewBox = "0 0 1000 500";
        public const string SvgNamespace = "http://www.w3.org/2000/svg";

        // Layout parametry
        public const int SvgMarginX = 20;

        public const int SvgRowHeight = 200;

        public const int SvgRowPadding = 15;

        public static class SvgRows
        {
            public const int Row1Y = 20;

            public const int Row2Y = 20 + SvgRowHeight + SvgRowPadding;
        }

        public static readonly (string prefix, int yPosition)[] RowConfigurations = new[]
        {
            ("SJ", SvgRows.Row1Y),      // Øádek 1: Sekce Jih
            ("SS", SvgRows.Row2Y)       // Øádek 2: Sekce Sever
        };

        public static readonly SekceMapConfig[] Sekce = new[]
        {
            // Horní øada: 4 sekce (jih)
            new SekceMapConfig { Id = 1, AnchorId = "SJ1", X = 20, Y = 20, Width = 220, Height = 200, LabelX = 130, LabelY = 120, Nazev = "Sekce jih 1" },
            new SekceMapConfig { Id = 2, AnchorId = "SJ2", X = 260, Y = 20, Width = 220, Height = 200, LabelX = 370, LabelY = 120, Nazev = "Sekce jih 2" },
            new SekceMapConfig { Id = 3, AnchorId = "SJ3", X = 500, Y = 20, Width = 220, Height = 200, LabelX = 610, LabelY = 120, Nazev = "Sekce jih 3" },
            new SekceMapConfig { Id = 4, AnchorId = "SJ4", X = 740, Y = 20, Width = 220, Height = 200, LabelX = 850, LabelY = 120, Nazev = "Sekce jih 4" },

            // Dolní øada: 5 sekcí (sever)
            new SekceMapConfig { Id = 5, AnchorId = "SS1", X = 20, Y = 260, Width = 150, Height = 200, LabelX = 95, LabelY = 360, Nazev = "Sekce sever 1" },
            new SekceMapConfig { Id = 6, AnchorId = "SS2", X = 190, Y = 260, Width = 150, Height = 200, LabelX = 265, LabelY = 360, Nazev = "Sekce sever 2" },
            new SekceMapConfig { Id = 7, AnchorId = "SS3", X = 360, Y = 260, Width = 150, Height = 200, LabelX = 435, LabelY = 360, Nazev = "Sekce sever 3" },
            new SekceMapConfig { Id = 8, AnchorId = "SS4", X = 530, Y = 260, Width = 150, Height = 200, LabelX = 605, LabelY = 360, Nazev = "Sekce sever 4" },
            new SekceMapConfig { Id = 9, AnchorId = "SS5", X = 700, Y = 260, Width = 150, Height = 200, LabelX = 775, LabelY = 360, Nazev = "Sekce sever 5" }
        };
    }
}
