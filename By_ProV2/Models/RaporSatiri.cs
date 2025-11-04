using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace By_ProV2.Models
{
    public class RaporSatiri
    {
        public string BelgeKodu { get; set; }
        public DateTime TeslimTarihi { get; set; }
        public string CariKodu { get; set; }
        public string CariAdi { get; set; }
        public decimal AlisTutari { get; set; }
        public decimal SatisTutari { get; set; }

        public decimal BrutKar => SatisTutari - AlisTutari;
        public decimal KarYuzdesi => SatisTutari > 0 ? (BrutKar / SatisTutari) * 100 : 0;
    }

}
