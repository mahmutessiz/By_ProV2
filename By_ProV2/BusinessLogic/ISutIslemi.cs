using System.Collections.ObjectModel;
using By_ProV2.Models;

namespace By_ProV2.BusinessLogic
{
    public interface ISutIslemi
    {
        void Kaydet(ObservableCollection<SutKaydi> liste);
    }
}
