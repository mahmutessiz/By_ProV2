using System.Collections.ObjectModel;
using By_ProV2.Models;

namespace By_ProV2.BusinessLogic
{
    public class SutIslemContext
    {
        private readonly ISutIslemi _islem;

        public SutIslemContext(ISutIslemi islem)
        {
            _islem = islem;
        }

        public void Kaydet(ObservableCollection<SutKaydi> liste)
        {
            _islem.Kaydet(liste);
        }
    }
}
