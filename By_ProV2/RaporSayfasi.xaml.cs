using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using By_ProV2.Models;
using System.IO;

namespace By_ProV2
{
    /// <summary>
    /// RaporSayfasi.xaml etkileşim mantığı
    /// </summary>
    public partial class RaporSayfasi : Window
    {
        private RaporViewModel _vm;

        public RaporSayfasi()
        {
            InitializeComponent();
            _vm = new RaporViewModel();
            this.DataContext = _vm;
        }

        private async void BtnRaporYukle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _vm.YukleAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Rapor yükleme hatası:\n" + ex.Message);
            }
        }
        private void BtnPdfOlustur_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as RaporViewModel;

            // Liste boşsa engelle
            if (viewModel.Rapor.Count == 0)
            {
                MessageBox.Show("Önce raporu getirin.");
                return;
            }

            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Rapor.pdf");

            PdfRaporOlusturucu olusturucu = new PdfRaporOlusturucu();
            olusturucu.Olustur(viewModel.Rapor.ToList(), path, viewModel.BaslangicTarihi, viewModel.BitisTarihi);
        }
    }

}