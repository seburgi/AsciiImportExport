using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsciiImportExport.Tests.Pocos
{
    public class GpsLocation
    {
        public GpsLocation(double lat, double lon)
        {
            Lat = lat;
            Lon = lon;
        }

        public double Lat { get; private set; }
        public double Lon { get; private set; }
    }
}
