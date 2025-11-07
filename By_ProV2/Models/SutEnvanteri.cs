using System;

namespace By_ProV2.Models
{
    public class SutEnvanteri
    {
        public int Id { get; set; }
        public DateTime Tarih { get; set; }
        
        // Opening stock from previous day
        public decimal DevirSut { get; set; }
        
        // Daily received milk (from purchases)
        public decimal GunlukAlinanSut { get; set; }
        
        // Daily sold milk (from sales)
        public decimal GunlukSatilanSut { get; set; }
        
        // Calculated closing stock: Devir + Alınan - Satılan
        public decimal KalanSut { get; set; }
        
        public string Aciklama { get; set; }
        
        // Tracking information
        public int? CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime ModifiedAt { get; set; } = DateTime.Now;
    }
}