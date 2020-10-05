using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPPipAPI_SSOM.Models
{
    public class clsIOTGeneral
    {

     /*    "timestamp": "2020-09-21T02:02:45Z",
    "type": "Unknown",
    "mac": "AC233F545F65",
    "bleName": "",
    "rssi": -48,
    "rawData":*/
    }

    public class IotDevice
    {
        public string timestamp { get; set; }
        public string type { get; set; }
        public string mac { get; set; }
        public string bleName { get; set; }
        public string rssi { get; set; }
        public string ibeaconTxPower { get; set; }
        public string rawData { get; set; }
       
    }
}