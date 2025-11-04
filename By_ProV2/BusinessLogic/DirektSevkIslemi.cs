using System.Collections.ObjectModel;
using By_ProV2.Models;
using By_ProV2.DataAccess;

namespace By_ProV2.BusinessLogic
{
    public class DirektSevkIslemi : ISutIslemi
    {
        public void Kaydet(ObservableCollection<SutKaydi> liste)
        {
            var repo = new SutRepository();
            foreach (var kayit in liste)
            {
                kayit.IslemTuru = "Direkt Sevk";
                repo.KaydetSutKaydi(kayit);
            }
        }
    }
}
