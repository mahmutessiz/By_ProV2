using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using By_ProV2.DataAccess;
using By_ProV2.Models;

namespace By_ProV2
{
    public partial class ParametrelerWindow : Window
    {
        private readonly ParameterRepository _repo = new ParameterRepository();
        public ObservableCollection<Parameter> ParametreListesi { get; set; }

        public ParametrelerWindow()
        {
            InitializeComponent();
            ParametreListesi = new ObservableCollection<Parameter>();
            dgParametreler.ItemsSource = ParametreListesi;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadParametreler();
        }

        private void LoadParametreler()
        {
            try
            {
                var parametreler = _repo.GetAllParametreler();
                ParametreListesi.Clear();
                foreach (var param in parametreler)
                {
                    ParametreListesi.Add(param);
                }
                
                // Load the most recent parameters to the input fields
                var latestParam = _repo.GetLatestParametreler();
                if (latestParam != null)
                {
                    txtYagKesinti.Text = latestParam.YagKesintiParametresi?.ToString();
                    txtProtein.Text = latestParam.ProteinParametresi?.ToString();
                    txtDizemBasiTl.Text = latestParam.DizemBasiTl?.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Parametreler yüklenirken hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var tr = new CultureInfo("tr-TR");
                
                // Validate inputs
                if (!decimal.TryParse(txtYagKesinti.Text, NumberStyles.Any, tr, out decimal yagKesinti) ||
                    !decimal.TryParse(txtProtein.Text, NumberStyles.Any, tr, out decimal protein) ||
                    !decimal.TryParse(txtDizemBasiTl.Text, NumberStyles.Any, tr, out decimal dizemBasiTl))
                {
                    MessageBox.Show("Lütfen tüm parametre alanlarını doğru formatta girin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create new parameter
                var param = new Parameter
                {
                    YagKesintiParametresi = yagKesinti,
                    ProteinParametresi = protein,
                    DizemBasiTl = dizemBasiTl
                };

                // Save the parameter - this will update existing records if they exist
                _repo.KaydetParametre(param);
                
                MessageBox.Show("Parametreler başarıyla kaydedildi.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Reload the list
                LoadParametreler();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Parametreler kaydedilirken hata oluştu:\n{ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnVazgec_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dgParametreler_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // When a row is selected, populate the input fields
            if (dgParametreler.SelectedItem is Parameter selectedParam)
            {
                txtYagKesinti.Text = selectedParam.YagKesintiParametresi?.ToString();
                txtProtein.Text = selectedParam.ProteinParametresi?.ToString();
                txtDizemBasiTl.Text = selectedParam.DizemBasiTl?.ToString();
            }
        }
    }
}