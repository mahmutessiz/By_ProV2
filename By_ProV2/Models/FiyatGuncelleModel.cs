using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace By_ProV2.Models
{
    public class FiyatGuncelleModel
    {
        public int STOKID { get; set; }
        public string STOKADI { get; set; }
        public string STOKKODU { get; set; }
        public string LISTEADI { get; set; }
        public DateTime LISTETARIHI { get; set; }

        public decimal ALISFIYAT1 { get; set; }
        public decimal ALISFIYAT2 { get; set; }
        public decimal ALISFIYAT3 { get; set; }
        public decimal ALISFIYAT4 { get; set; }
        public decimal ALISFIYAT5 { get; set; }

        public decimal KDVORANI { get; set; }
        public string PARABIRIMI { get; set; }

        // UI'da kullanılacak ekstra alan
        public bool DegistiMi { get; set; } = false;
    }
}
