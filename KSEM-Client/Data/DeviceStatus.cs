using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSEM_Client.Data
{
    public class DeviceStatus
    {
        public string? Status { get; set; }

        public int? RamTotal { get; set; }
        public int? RamFree { get; set; }

        public int? FlashDataTotal { get; set; }
        public int? FlashDataFree { get; set; }

        public int? FlashAppTotal { get; set; }
        public int? FlashAppFree { get; set; }

        public int? CpuLoad { get; set; }
        public int? CpuTemp { get; set; }


    }
}
