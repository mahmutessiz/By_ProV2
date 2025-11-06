using System;

namespace By_ProV2.Models
{
    public class Parameter
    {
        public int ParametreId { get; set; }
        public decimal? YagKesintiParametresi { get; set; }
        public decimal? ProteinParametresi { get; set; }
        public decimal? DizemBasiTl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}