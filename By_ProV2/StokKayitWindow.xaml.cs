using System;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using By_ProV2.Helpers;


namespace By_ProV2
{
    public partial class StokKayitWindow : Window
    {
        // Public properties to access controls from outside the class
        public TextBox TxtStokKodu => txtStokKodu;
        public TextBox TxtStokAdi => txtStokAdi;
        public ComboBox CmbBirim => cmbBirim;
        public TextBox TxtAgirlik => txtAgirlik;
        public TextBox TxtProtein => txtProtein;
        public TextBox TxtEnerji => txtEnerji;
        public TextBox TxtNemOrani => txtNemOrani;
        public TextBox TxtBarkod => txtBarkod;
        public ComboBox CmbYemOzelligi => cmbYemOzelligi;
        public TextBox TxtAciklama => txtAciklama;
        public ComboBox CmbMensei => cmbMensei;
        public TextBox TxtAlisFiyat => txtAlisFiyat;
        public TextBox TxtAlisFiyat2 => txtAlisFiyat2;
        public TextBox TxtAlisFiyat3 => txtAlisFiyat3;
        public TextBox TxtAlisFiyat4 => txtAlisFiyat4;
        public TextBox TxtAlisFiyat5 => txtAlisFiyat5;
        public TextBox TxtKDV => txtKDV;
        public DatePicker DpListeTarihi => dpListeTarihi;
        public DatePicker DpIslemTarihi => dpIslemTarihi;
        public ComboBox CmbDepo => cmbDepo;
        public TextBox TxtDosyaYolu => txtDosyaYolu;

        public StokKayitWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dpIslemTarihi.SelectedDate = DateTime.Now;
            // Use the current authenticated user's username instead of the Windows environment username
            var currentUser = App.AuthService?.CurrentUser;
            txtOlusturan.Text = currentUser?.Username ?? Environment.UserName;
            TedarikciComboDoldur();
        }

        private void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStokKodu.Text))
            {
                MessageBox.Show("Stok kodu boş bırakılamaz.");
                return;
            }

            string connStr = ConfigurationHelper.GetConnectionString("db");

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Önceden aynı stok kodu var mı kontrol et
                    SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM STOKSABITKART WHERE STOKKODU = @STOKKODU", conn);
                    checkCmd.Parameters.AddWithValue("@STOKKODU", txtStokKodu.Text);

                    int existingCount = (int)checkCmd.ExecuteScalar();
                    if (existingCount > 0)
                    {
                        MessageBox.Show("Bu stok kodu zaten kayıtlı. Lütfen farklı bir stok kodu giriniz.");
                        return;
                    }

                    SqlTransaction trans = conn.BeginTransaction();

                   
                    try
                    {
                        // 1. STOKSABITKART
                        SqlCommand cmdStok = new SqlCommand(@"
INSERT INTO STOKSABITKART 
(STOKKODU, STOKADI, BIRIM, AGIRLIK, PROTEIN, ENERJI, NEM, BARKOD, YEMOZELLIGI, ACIKLAMA, MENSEI, AKTIF, OLUSTURMATARIHI, CreatedBy, ModifiedBy, CreatedAt, ModifiedAt)
VALUES 
(@STOKKODU, @STOKADI, @BIRIM, @AGIRLIK, @PROTEIN, @ENERJI, @NEM, @BARKOD, @YEMOZELLIGI, @ACIKLAMA, @MENSEI, @AKTIF, @OLUSTURMATARIHI, @CreatedBy, @ModifiedBy, @CreatedAt, @ModifiedAt);
SELECT SCOPE_IDENTITY();", conn, trans);

                        cmdStok.Parameters.AddWithValue("@STOKKODU", txtStokKodu.Text);
                        cmdStok.Parameters.AddWithValue("@STOKADI", txtStokAdi.Text);
                        cmdStok.Parameters.AddWithValue("@BIRIM", GetComboBoxValue(cmbBirim));
                        cmdStok.Parameters.AddWithValue("@AGIRLIK", ParseDecimal(txtAgirlik.Text));
                        cmdStok.Parameters.AddWithValue("@PROTEIN", ParseDecimal(txtProtein.Text));
                        cmdStok.Parameters.AddWithValue("@ENERJI", ParseDecimal(txtEnerji.Text));
                        cmdStok.Parameters.AddWithValue("@NEM", ParseDecimal(txtNemOrani.Text));
                        cmdStok.Parameters.AddWithValue("@BARKOD", txtBarkod.Text);
                        cmdStok.Parameters.AddWithValue("@YEMOZELLIGI", GetComboBoxValue(cmbYemOzelligi));
                        cmdStok.Parameters.AddWithValue("@ACIKLAMA", txtAciklama.Text);
                        cmdStok.Parameters.AddWithValue("@MENSEI", GetComboBoxValue(cmbMensei)); // Yerli/İthal
                        cmdStok.Parameters.AddWithValue("@AKTIF", 1);
                        cmdStok.Parameters.AddWithValue("@OLUSTURMATARIHI", dpIslemTarihi.SelectedDate ?? DateTime.Now);
                        
                        // Add user tracking parameters
                        var currentUserForStok = App.AuthService?.CurrentUser;
                        if (currentUserForStok != null)
                        {
                            cmdStok.Parameters.AddWithValue("@CreatedBy", currentUserForStok.Id);
                            cmdStok.Parameters.AddWithValue("@ModifiedBy", currentUserForStok.Id);
                        }
                        else
                        {
                            cmdStok.Parameters.AddWithValue("@CreatedBy", DBNull.Value);
                            cmdStok.Parameters.AddWithValue("@ModifiedBy", DBNull.Value);
                        }
                        cmdStok.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        cmdStok.Parameters.AddWithValue("@ModifiedAt", DateTime.Now);

                        // Otomatik oluşturulan STOKID'yi al
                        int stokId = Convert.ToInt32(cmdStok.ExecuteScalar());

                        // 2. STOKSABITFIYAT
                        SqlCommand cmdFiyat = new SqlCommand(@"
INSERT INTO STOKSABITFIYAT 
(STOKID, LISTEADI, LISTETARIHI, ALISFIYAT1, ALISFIYAT2, ALISFIYAT3, ALISFIYAT4, ALISFIYAT5, KDVORANI, PARABIRIMI, AKTIF, KAYITTARIHI, OLUSTURANKULLANICI, CreatedBy, ModifiedBy, CreatedAt, ModifiedAt)
VALUES 
(@STOKID, @LISTEADI, @LISTETARIHI, @ALISFIYAT1,@ALISFIYAT2, @ALISFIYAT3, @ALISFIYAT4, @ALISFIYAT5,  @KDVORANI, @PARABIRIMI, @AKTIF, @KAYITTARIHI, @OLUSTURANKULLANICI, @CreatedBy, @ModifiedBy, @CreatedAt, @ModifiedAt)", conn, trans);

                        cmdFiyat.Parameters.AddWithValue("@STOKID", stokId);
                        cmdFiyat.Parameters.AddWithValue("@LISTEADI", "Standart");
                        cmdFiyat.Parameters.AddWithValue("@LISTETARIHI", dpListeTarihi.SelectedDate ?? DateTime.Now);
                        cmdFiyat.Parameters.AddWithValue("@ALISFIYAT1", ParseDecimal(txtAlisFiyat.Text));
                        cmdFiyat.Parameters.AddWithValue("@ALISFIYAT2", ParseDecimal(txtAlisFiyat2.Text));
                        cmdFiyat.Parameters.AddWithValue("@ALISFIYAT3", ParseDecimal(txtAlisFiyat3.Text));
                        cmdFiyat.Parameters.AddWithValue("@ALISFIYAT4", ParseDecimal(txtAlisFiyat4.Text));
                        cmdFiyat.Parameters.AddWithValue("@ALISFIYAT5", ParseDecimal(txtAlisFiyat5.Text));
                        cmdFiyat.Parameters.AddWithValue("@KDVORANI", ParseDecimal(txtKDV.Text));
                        cmdFiyat.Parameters.AddWithValue("@PARABIRIMI", "TRY"); // Sabit
                        cmdFiyat.Parameters.AddWithValue("@AKTIF", 1);
                        cmdFiyat.Parameters.AddWithValue("@KAYITTARIHI", DateTime.Now);
                        cmdFiyat.Parameters.AddWithValue("@OLUSTURANKULLANICI", txtOlusturan.Text);
                        
                        // Add user tracking parameters
                        var currentUserForFiyat = App.AuthService?.CurrentUser;
                        if (currentUserForFiyat != null)
                        {
                            cmdFiyat.Parameters.AddWithValue("@CreatedBy", currentUserForFiyat.Id);
                            cmdFiyat.Parameters.AddWithValue("@ModifiedBy", currentUserForFiyat.Id);
                        }
                        else
                        {
                            cmdFiyat.Parameters.AddWithValue("@CreatedBy", DBNull.Value);
                            cmdFiyat.Parameters.AddWithValue("@ModifiedBy", DBNull.Value);
                        }
                        cmdFiyat.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        cmdFiyat.Parameters.AddWithValue("@ModifiedAt", DateTime.Now);

                        cmdFiyat.ExecuteNonQuery();

                        // 3. STOKSABITBELGE
                        SqlCommand cmdBelge = new SqlCommand(@"
INSERT INTO STOKSABITBELGE 
(STOKID, BELGETIPI, DOSYAYOLU, EKLEMETARIHI, CreatedBy, ModifiedBy, CreatedAt, ModifiedAt)
VALUES 
(@STOKID, @BELGETIPI, @DOSYAYOLU, @EKLEMETARIHI, @CreatedBy, @ModifiedBy, @CreatedAt, @ModifiedAt)", conn, trans);

                        cmdBelge.Parameters.AddWithValue("@STOKID", stokId);
                        cmdBelge.Parameters.AddWithValue("@BELGETIPI", "Tanıtım");
                        cmdBelge.Parameters.AddWithValue("@DOSYAYOLU", string.IsNullOrEmpty(txtDosyaYolu.Text) ? (object)DBNull.Value : txtDosyaYolu.Text);
                        cmdBelge.Parameters.AddWithValue("@EKLEMETARIHI", DateTime.Now);
                        
                        // Add user tracking parameters
                        var currentUserForBelge = App.AuthService?.CurrentUser;
                        if (currentUserForBelge != null)
                        {
                            cmdBelge.Parameters.AddWithValue("@CreatedBy", currentUserForBelge.Id);
                            cmdBelge.Parameters.AddWithValue("@ModifiedBy", currentUserForBelge.Id);
                        }
                        else
                        {
                            cmdBelge.Parameters.AddWithValue("@CreatedBy", DBNull.Value);
                            cmdBelge.Parameters.AddWithValue("@ModifiedBy", DBNull.Value);
                        }
                        cmdBelge.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        cmdBelge.Parameters.AddWithValue("@ModifiedAt", DateTime.Now);

                        cmdBelge.ExecuteNonQuery();


                        // 4. STOKSABITHAREKET (İlk giriş hareketi)
                        SqlCommand cmdHareket = new SqlCommand(@"
INSERT INTO STOKSABITHAREKET 
(STOKID, HAREKETTURU, MIKTAR, BIRIM, DEPOID, ISLEMTARIHI, CreatedBy, ModifiedBy, CreatedAt, ModifiedAt)
VALUES 
(@STOKID, @HAREKETTURU, @MIKTAR, @BIRIM, @DEPOID, @ISLEMTARIHI, @CreatedBy, @ModifiedBy, @CreatedAt, @ModifiedAt)", conn, trans);

                        cmdHareket.Parameters.AddWithValue("@STOKID", stokId);
                        cmdHareket.Parameters.AddWithValue("@HAREKETTURU", "Giriş");
                        cmdHareket.Parameters.AddWithValue("@MIKTAR", 0);
                        cmdHareket.Parameters.AddWithValue("@BIRIM", GetComboBoxValue(cmbBirim));
                        cmdHareket.Parameters.AddWithValue("@DEPOID", cmbDepo.SelectedValue ?? DBNull.Value);
                        cmdHareket.Parameters.AddWithValue("@ISLEMTARIHI", DateTime.Now);
                        
                        // Add user tracking parameters
                        var currentUserForHareket = App.AuthService?.CurrentUser;
                        if (currentUserForHareket != null)
                        {
                            cmdHareket.Parameters.AddWithValue("@CreatedBy", currentUserForHareket.Id);
                            cmdHareket.Parameters.AddWithValue("@ModifiedBy", currentUserForHareket.Id);
                        }
                        else
                        {
                            cmdHareket.Parameters.AddWithValue("@CreatedBy", DBNull.Value);
                            cmdHareket.Parameters.AddWithValue("@ModifiedBy", DBNull.Value);
                        }
                        cmdHareket.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                        cmdHareket.Parameters.AddWithValue("@ModifiedAt", DateTime.Now);

                        cmdHareket.ExecuteNonQuery();

                        trans.Commit();
                        MessageBox.Show("Stok başarıyla kaydedildi.");
                        TemizleForm();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Kayıt sırasında hata oluştu: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bağlantı hatası: " + ex.Message);
            }
        }
        private decimal ParseDecimal(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0;

            // Nokta veya virgül fark etmesin diye normalize et
            input = input.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                         .Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal result))
                return result;

            MessageBox.Show($"Geçersiz ondalık sayı: {input}");
            return 0;
        }
       
        private string GetComboBoxValue(ComboBox comboBox)
        {
            return (comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
        }

        private void TemizleForm()
        {
            txtStokKodu.Clear();
            txtStokAdi.Clear();
            cmbBirim.SelectedIndex = -1;
            txtAgirlik.Clear();
            txtProtein.Clear();
            txtEnerji.Clear();
            txtNemOrani.Clear();
            txtBarkod.Clear();
            cmbYemOzelligi.SelectedIndex = -1;
            txtAciklama.Clear();
            cmbMensei.SelectedIndex = -1;
            cmbDepo.SelectedIndex = -1;
            cmbTedarikci.SelectedIndex = -1;
            txtPartiNo.Clear();
            txtFaturaNo.Clear();
            txtTeslimAlan.Clear();
            txtPlaka.Clear();
            txtSoforAdi.Clear();
            txtAlisFiyat.Clear();
            txtAlisFiyat2.Clear();
            txtAlisFiyat3.Clear();
            txtAlisFiyat4.Clear();
            txtAlisFiyat5.Clear();
            txtAciklama1.Clear();
            txtDosyaYolu.Text = string.Empty;
            txtMuhasebeKodu.Clear();
            txtKDV.Clear();
            // Use the current authenticated user's username instead of the Windows environment username
            var currentUser = App.AuthService?.CurrentUser;
            txtOlusturan.Text = currentUser?.Username ?? Environment.UserName;
            dpIslemTarihi.SelectedDate = DateTime.Now;
            dpListeTarihi.SelectedDate = DateTime.Now;
        }

        private void btnDosyaSec_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog();
            openFile.Filter = "PDF Files (*.pdf)|*.pdf|JPG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|All Files (*.*)|*.*";

            if (openFile.ShowDialog() == true)
            {
                txtDosyaYolu.Text = openFile.FileName;
                MessageBox.Show("Dosya seçildi: " + txtDosyaYolu.Text);
            }
        }
        private void btnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStokKodu.Text))
            {
                MessageBox.Show("Güncellenecek stok kodu girilmelidir.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStokKodu.Focus();
                return;
            }

            string connStr = ConfigurationHelper.GetConnectionString("db");

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // 1. STOKID'yi al
                    SqlCommand getIdCmd = new SqlCommand("SELECT STOKID FROM STOKSABITKART WHERE STOKKODU = @STOKKODU", conn);
                    getIdCmd.Parameters.AddWithValue("@STOKKODU", txtStokKodu.Text);

                    object result = getIdCmd.ExecuteScalar();
                    if (result == null)
                    {
                        MessageBox.Show("Bu stok kodu bulunamadı. Lütfen geçerli bir stok kodu giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    int stokId = Convert.ToInt32(result);

                    SqlTransaction trans = conn.BeginTransaction();

                    try
                    {
                        // 1. STOKSABITKART güncelle
                        SqlCommand cmdKart = new SqlCommand(@"
UPDATE STOKSABITKART SET
    STOKADI = @STOKADI,
    BIRIM = @BIRIM,
    AGIRLIK = @AGIRLIK,
    PROTEIN = @PROTEIN,
    ENERJI = @ENERJI,
    NEM = @NEM,
    BARKOD = @BARKOD,
    YEMOZELLIGI = @YEMOZELLIGI,
    ACIKLAMA = @ACIKLAMA,
    MENSEI = @MENSEI,
    AKTIF = @AKTIF,
    OLUSTURMATARIHI = @OLUSTURMATARIHI,
    ModifiedBy = @ModifiedBy,
    ModifiedAt = @ModifiedAt
WHERE STOKID = @STOKID", conn, trans);

                        cmdKart.Parameters.AddWithValue("@STOKADI", txtStokAdi.Text);
                        cmdKart.Parameters.AddWithValue("@BIRIM", GetComboBoxValue(cmbBirim));
                        cmdKart.Parameters.AddWithValue("@AGIRLIK", ParseDecimal(txtAgirlik.Text));
                        cmdKart.Parameters.AddWithValue("@PROTEIN", ParseDecimal(txtProtein.Text));
                        cmdKart.Parameters.AddWithValue("@ENERJI", ParseDecimal(txtEnerji.Text));
                        cmdKart.Parameters.AddWithValue("@NEM", ParseDecimal(txtNemOrani.Text));
                        cmdKart.Parameters.AddWithValue("@BARKOD", txtBarkod.Text);
                        cmdKart.Parameters.AddWithValue("@YEMOZELLIGI", GetComboBoxValue(cmbYemOzelligi));
                        cmdKart.Parameters.AddWithValue("@ACIKLAMA", txtAciklama.Text);
                        cmdKart.Parameters.AddWithValue("@MENSEI", GetComboBoxValue(cmbMensei));
                        cmdKart.Parameters.AddWithValue("@AKTIF", 1);
                        cmdKart.Parameters.AddWithValue("@OLUSTURMATARIHI", dpIslemTarihi.SelectedDate ?? DateTime.Now);
                        cmdKart.Parameters.AddWithValue("@STOKID", stokId);
                        
                        // Add user tracking parameters for update
                        var currentUserForKart = App.AuthService?.CurrentUser;
                        if (currentUserForKart != null)
                        {
                            cmdKart.Parameters.AddWithValue("@ModifiedBy", currentUserForKart.Id);
                        }
                        else
                        {
                            cmdKart.Parameters.AddWithValue("@ModifiedBy", DBNull.Value);
                        }
                        cmdKart.Parameters.AddWithValue("@ModifiedAt", DateTime.Now);

                        cmdKart.ExecuteNonQuery();

                        // 2. STOKSABITFIYAT - eğer varsa güncelle, yoksa ekle
                        SqlCommand fiyatCheckCmd = new SqlCommand("SELECT COUNT(*) FROM STOKSABITFIYAT WHERE STOKID = @STOKID AND LISTEADI = 'Standart'", conn, trans);
                        fiyatCheckCmd.Parameters.AddWithValue("@STOKID", stokId);

                        int fiyatVar = (int)fiyatCheckCmd.ExecuteScalar();

                        if (fiyatVar > 0)
                        {
                            SqlCommand cmdFiyatGuncelle = new SqlCommand(@"
UPDATE STOKSABITFIYAT SET
    LISTETARIHI = @LISTETARIHI,
    ALISFIYAT1 = @ALISFIYAT1,
    ALISFIYAT2 = @ALISFIYAT2,
    ALISFIYAT3 = @ALISFIYAT3,
    ALISFIYAT4 = @ALISFIYAT4,
    ALISFIYAT5 = @ALISFIYAT5,
    KDVORANI = @KDVORANI,
    PARABIRIMI = @PARABIRIMI,
    AKTIF = @AKTIF,
    KAYITTARIHI = @KAYITTARIHI,
    OLUSTURANKULLANICI = @OLUSTURANKULLANICI,
    ModifiedBy = @ModifiedBy,
    ModifiedAt = @ModifiedAt
WHERE STOKID = @STOKID AND LISTEADI = 'Standart'", conn, trans);

                            cmdFiyatGuncelle.Parameters.AddWithValue("@LISTETARIHI", dpListeTarihi.SelectedDate ?? DateTime.Now);
                            cmdFiyatGuncelle.Parameters.AddWithValue("@ALISFIYAT1", ParseDecimal(txtAlisFiyat.Text));
                            cmdFiyatGuncelle.Parameters.AddWithValue("@ALISFIYAT2", ParseDecimal(txtAlisFiyat2.Text));
                            cmdFiyatGuncelle.Parameters.AddWithValue("@ALISFIYAT3", ParseDecimal(txtAlisFiyat3.Text));
                            cmdFiyatGuncelle.Parameters.AddWithValue("@ALISFIYAT4", ParseDecimal(txtAlisFiyat4.Text));
                            cmdFiyatGuncelle.Parameters.AddWithValue("@ALISFIYAT5", ParseDecimal(txtAlisFiyat5.Text));
                            cmdFiyatGuncelle.Parameters.AddWithValue("@KDVORANI", ParseDecimal(txtKDV.Text));
                            cmdFiyatGuncelle.Parameters.AddWithValue("@PARABIRIMI", "TRY");
                            cmdFiyatGuncelle.Parameters.AddWithValue("@AKTIF", 1);
                            cmdFiyatGuncelle.Parameters.AddWithValue("@KAYITTARIHI", DateTime.Now);
                            cmdFiyatGuncelle.Parameters.AddWithValue("@OLUSTURANKULLANICI", txtOlusturan.Text);
                            cmdFiyatGuncelle.Parameters.AddWithValue("@STOKID", stokId);
                            
                            // Add user tracking parameters for update
                            var currentUserForFiyat = App.AuthService?.CurrentUser;
                            if (currentUserForFiyat != null)
                            {
                                cmdFiyatGuncelle.Parameters.AddWithValue("@ModifiedBy", currentUserForFiyat.Id);
                            }
                            else
                            {
                                cmdFiyatGuncelle.Parameters.AddWithValue("@ModifiedBy", DBNull.Value);
                            }
                            cmdFiyatGuncelle.Parameters.AddWithValue("@ModifiedAt", DateTime.Now);

                            cmdFiyatGuncelle.ExecuteNonQuery();
                        }
                        else
                        {
                            SqlCommand cmdFiyatEkle = new SqlCommand(@"
INSERT INTO STOKSABITFIYAT 
(STOKID, LISTEADI, LISTETARIHI, ALISFIYAT1, ALISFIYAT2, ALISFIYAT3, ALISFIYAT4, ALISFIYAT5, KDVORANI, PARABIRIMI, AKTIF, KAYITTARIHI, OLUSTURANKULLANICI)
VALUES 
(@STOKID, @LISTEADI, @LISTETARIHI, @ALISFIYAT1, @ALISFIYAT2, @ALISFIYAT3, @ALISFIYAT4, @ALISFIYAT5, @KDVORANI, @PARABIRIMI, @AKTIF, @KAYITTARIHI, @OLUSTURANKULLANICI)", conn, trans);

                            cmdFiyatEkle.Parameters.AddWithValue("@STOKID", stokId);
                            cmdFiyatEkle.Parameters.AddWithValue("@LISTEADI", "Standart");
                            cmdFiyatEkle.Parameters.AddWithValue("@LISTETARIHI", dpListeTarihi.SelectedDate ?? DateTime.Now);
                            cmdFiyatEkle.Parameters.AddWithValue("@ALISFIYAT1", ParseDecimal(txtAlisFiyat.Text));
                            cmdFiyatEkle.Parameters.AddWithValue("@ALISFIYAT2", ParseDecimal(txtAlisFiyat2.Text));
                            cmdFiyatEkle.Parameters.AddWithValue("@ALISFIYAT3", ParseDecimal(txtAlisFiyat3.Text));
                            cmdFiyatEkle.Parameters.AddWithValue("@ALISFIYAT4", ParseDecimal(txtAlisFiyat4.Text));
                            cmdFiyatEkle.Parameters.AddWithValue("@ALISFIYAT5", ParseDecimal(txtAlisFiyat5.Text));
                            cmdFiyatEkle.Parameters.AddWithValue("@KDVORANI", ParseDecimal(txtKDV.Text));
                            cmdFiyatEkle.Parameters.AddWithValue("@PARABIRIMI", "TRY");
                            cmdFiyatEkle.Parameters.AddWithValue("@AKTIF", 1);
                            cmdFiyatEkle.Parameters.AddWithValue("@KAYITTARIHI", DateTime.Now);
                            cmdFiyatEkle.Parameters.AddWithValue("@OLUSTURANKULLANICI", txtOlusturan.Text);

                            cmdFiyatEkle.ExecuteNonQuery();
                        }

                        // 3. STOKSABITBELGE - önce sil, sonra yeniden ekle (tek belge varsa)
                        SqlCommand cmdDeleteBelge = new SqlCommand("DELETE FROM STOKSABITBELGE WHERE STOKID = @STOKID", conn, trans);
                        cmdDeleteBelge.Parameters.AddWithValue("@STOKID", stokId);
                        cmdDeleteBelge.ExecuteNonQuery();

                        SqlCommand cmdBelge = new SqlCommand(@"
INSERT INTO STOKSABITBELGE 
(STOKID, BELGETIPI, DOSYAYOLU, EKLEMETARIHI)
VALUES 
(@STOKID, @BELGETIPI, @DOSYAYOLU, @EKLEMETARIHI)", conn, trans);

                        cmdBelge.Parameters.AddWithValue("@STOKID", stokId);
                        cmdBelge.Parameters.AddWithValue("@BELGETIPI", "Tanıtım");
                        cmdBelge.Parameters.AddWithValue("@DOSYAYOLU", string.IsNullOrEmpty(txtDosyaYolu.Text) ? (object)DBNull.Value : txtDosyaYolu.Text);
                        cmdBelge.Parameters.AddWithValue("@EKLEMETARIHI", DateTime.Now);

                        cmdBelge.ExecuteNonQuery();

                        // Commit tüm işlemler
                        trans.Commit();

                        MessageBox.Show("Stok başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        TemizleForm();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Güncelleme sırasında hata oluştu:\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı bağlantı hatası:\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSil_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStokKodu.Text))
            {
                MessageBox.Show("Silinecek stok kodu girilmelidir.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStokKodu.Focus();
                return;
            }

            var cevap = MessageBox.Show("Bu stok kaydını ve tüm ilişkili verileri silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (cevap != MessageBoxResult.Yes)
                return;

            string connStr = ConfigurationHelper.GetConnectionString("db");

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlTransaction trans = conn.BeginTransaction();

                    try
                    {
                        // Önce STOKID'yi al
                        SqlCommand cmdGetId = new SqlCommand("SELECT STOKID FROM STOKSABITKART WHERE STOKKODU = @STOKKODU", conn, trans);
                        cmdGetId.Parameters.AddWithValue("@STOKKODU", txtStokKodu.Text);
                        object stokIdObj = cmdGetId.ExecuteScalar();

                        if (stokIdObj == null)
                        {
                            MessageBox.Show("Stok kodu bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        int stokId = Convert.ToInt32(stokIdObj);

                        // Önce alt tabloları temizle
                        new SqlCommand("DELETE FROM STOKSABITFIYAT WHERE STOKID = @STOKID", conn, trans)
                            .AddParam("@STOKID", stokId).ExecuteNonQuery();

                        new SqlCommand("DELETE FROM STOKSABITBELGE WHERE STOKID = @STOKID", conn, trans)
                            .AddParam("@STOKID", stokId).ExecuteNonQuery();

                        new SqlCommand("DELETE FROM STOKSABITHAREKET WHERE STOKID = @STOKID", conn, trans)
                            .AddParam("@STOKID", stokId).ExecuteNonQuery();

                        // Son olarak ana kaydı sil
                        SqlCommand cmdDelete = new SqlCommand("DELETE FROM STOKSABITKART WHERE STOKID = @STOKID", conn, trans);
                        cmdDelete.Parameters.AddWithValue("@STOKID", stokId);

                        int result = cmdDelete.ExecuteNonQuery();
                        trans.Commit();

                        if (result > 0)
                        {
                            MessageBox.Show("Stok ve ilişkili veriler başarıyla silindi.", "Silindi", MessageBoxButton.OK, MessageBoxImage.Information);
                            TemizleForm();
                        }
                        else
                        {
                            MessageBox.Show("Silme işlemi başarısız. Kayıt bulunamadı.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        MessageBox.Show("Silme sırasında hata oluştu:\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bağlantı hatası:\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnTemizle_Click(object sender, RoutedEventArgs e)
        {
            TemizleForm();
        }
        private void txtStokKodu_LostFocus(object sender, RoutedEventArgs e)
        {
            string stokKodu = txtStokKodu.Text.Trim();
            if (string.IsNullOrEmpty(stokKodu)) return;

            string connStr = ConfigurationHelper.GetConnectionString("db");

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // STOKID'yi bul
                    SqlCommand cmdStokId = new SqlCommand("SELECT STOKID FROM STOKSABITKART WHERE STOKKODU = @STOKKODU", conn);
                    cmdStokId.Parameters.AddWithValue("@STOKKODU", stokKodu);
                    object stokIdObj = cmdStokId.ExecuteScalar();

                    if (stokIdObj == null)
                    {
                        MessageBox.Show("Bu stok kodu sistemde bulunamadı. Yeni kayıt olarak işleme devam edebilirsiniz.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        return;
                    }

                    int stokId = Convert.ToInt32(stokIdObj);

                    // 1. STOKSABITKART verileri
                    SqlCommand cmdKart = new SqlCommand(@"
                SELECT STOKADI, BIRIM, AGIRLIK, PROTEIN, ENERJI, NEM, BARKOD, YEMOZELLIGI, 
                       ACIKLAMA, MENSEI, OLUSTURMATARIHI 
                FROM STOKSABITKART WHERE STOKID = @STOKID", conn);
                    cmdKart.Parameters.AddWithValue("@STOKID", stokId);

                    using (SqlDataReader reader = cmdKart.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtStokAdi.Text = reader["STOKADI"]?.ToString() ?? "";
                            SetComboBoxValue(cmbBirim, reader["BIRIM"]?.ToString() ?? "");
                            txtAgirlik.Text = reader["AGIRLIK"]?.ToString();
                            txtProtein.Text = reader["PROTEIN"]?.ToString();
                            txtEnerji.Text = reader["ENERJI"]?.ToString();
                            txtNemOrani.Text = reader["NEM"]?.ToString();
                            txtBarkod.Text = reader["BARKOD"]?.ToString() ?? "";
                            SetComboBoxValue(cmbYemOzelligi, reader["YEMOZELLIGI"]?.ToString() ?? "");
                            txtAciklama.Text = reader["ACIKLAMA"]?.ToString() ?? "";
                            SetComboBoxValue(cmbMensei, reader["MENSEI"]?.ToString() ?? "");
                            dpIslemTarihi.SelectedDate = reader["OLUSTURMATARIHI"] as DateTime?;
                        }
                    }

                    // 2. STOKSABITFIYAT verileri (son kayıt)
                    SqlCommand cmdFiyat = new SqlCommand(@"
                SELECT TOP 1 ALISFIYAT1, ALISFIYAT2, ALISFIYAT3, ALISFIYAT4, ALISFIYAT5, KDVORANI, LISTETARIHI
                FROM STOKSABITFIYAT WHERE STOKID = @STOKID ORDER BY KAYITTARIHI DESC", conn);
                    cmdFiyat.Parameters.AddWithValue("@STOKID", stokId);

                    using (SqlDataReader reader = cmdFiyat.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtAlisFiyat.Text = reader["ALISFIYAT1"]?.ToString();
                            txtAlisFiyat2.Text = reader["ALISFIYAT2"]?.ToString();
                            txtAlisFiyat3.Text = reader["ALISFIYAT3"]?.ToString();
                            txtAlisFiyat4.Text = reader["ALISFIYAT4"]?.ToString();
                            txtAlisFiyat5.Text = reader["ALISFIYAT5"]?.ToString();
                            txtKDV.Text = reader["KDVORANI"]?.ToString();
                            dpListeTarihi.SelectedDate = reader["LISTETARIHI"] as DateTime?;
                        }
                    }

                    // 3. STOKSABITBELGE verileri (varsayalım: sadece "Tanıtım" tipi gösterilecek)
                    SqlCommand cmdBelge = new SqlCommand(@"
                SELECT TOP 1 DOSYAYOLU FROM STOKSABITBELGE 
                WHERE STOKID = @STOKID AND BELGETIPI = 'Tanıtım' 
                ORDER BY EKLEMETARIHI DESC", conn);
                    cmdBelge.Parameters.AddWithValue("@STOKID", stokId);
                    object dosyaYolu = cmdBelge.ExecuteScalar();
                    txtDosyaYolu.Text = dosyaYolu?.ToString() ?? "";

                    // 4. STOKSABITHAREKET verileri (son giriş hareketinden depo ID alalım)
                    SqlCommand cmdHareket = new SqlCommand(@"
                SELECT TOP 1 DEPOID FROM STOKSABITHAREKET 
                WHERE STOKID = @STOKID AND HAREKETTURU = 'Giriş' 
                ORDER BY ISLEMTARIHI DESC", conn);
                    cmdHareket.Parameters.AddWithValue("@STOKID", stokId);
                    object depoIdObj = cmdHareket.ExecuteScalar();
                    if (depoIdObj != null && depoIdObj != DBNull.Value)
                    {
                        cmbDepo.SelectedValue = Convert.ToInt32(depoIdObj);
                    }
                    else
                    {
                        cmbDepo.SelectedIndex = -1;
                    }

                    MessageBox.Show("Stok tüm verilerle birlikte yüklendi. Güncelleyebilirsiniz.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veri sorgulama hatası: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public void SetComboBoxValue(ComboBox comboBox, string value)
        {
            foreach (var item in comboBox.Items)
            {
                if (item is ComboBoxItem cbi && cbi.Content.ToString() == value)
                {
                    comboBox.SelectedItem = cbi;
                    break;
                }
            }
        }
        private void txtStokKodu_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Enter tuşuna basıldığında LostFocus işlemiyle aynı işlemi yap
                txtStokKodu_LostFocus(sender, null);

                // Enter tuşunun formda başka yere gitmesini engelle
                e.Handled = true;
            }
        }

        private void btnStokListe_Click(object sender, RoutedEventArgs e)
        {
            var liste = new StokListeWindow();
            if (liste.ShowDialog() == true)
            {
                var secilen = liste.SecilenStok;

                // Temel bilgiler
                txtStokKodu.Text = secilen.StokKodu;
                txtStokAdi.Text = secilen.StokAdi;
                SetComboBoxValue(cmbBirim, secilen.Birim);
                txtAgirlik.Text = secilen.Agirlik.ToString("0.##");
                txtProtein.Text = secilen.Protein.ToString("0.##");
                txtEnerji.Text = secilen.Enerji.ToString("0.##");
                txtNemOrani.Text = secilen.Nem.ToString("0.##");
                txtBarkod.Text = secilen.Barkod;
                SetComboBoxValue(cmbYemOzelligi, secilen.YemOzelligi);
                txtAciklama.Text = secilen.Aciklama;
                SetComboBoxValue(cmbMensei, secilen.Mensei);

                // Fiyatlar
                txtAlisFiyat.Text = secilen.AlisFiyat?.ToString("0.##");
                txtAlisFiyat2.Text = secilen.AlisFiyat2?.ToString("0.##");
                txtAlisFiyat3.Text = secilen.AlisFiyat3?.ToString("0.##");
                txtAlisFiyat4.Text = secilen.AlisFiyat4?.ToString("0.##");
                txtAlisFiyat5.Text = secilen.AlisFiyat5?.ToString("0.##");
                txtKDV.Text = secilen.KdvOrani?.ToString("0.##");
                dpListeTarihi.SelectedDate = secilen.ListeTarihi;

                // Dosya
                txtDosyaYolu.Text = secilen.DosyaYolu;

                // Depo / hareket
                if (secilen.IslemTarihi.HasValue)
                    dpIslemTarihi.SelectedDate = secilen.IslemTarihi.Value;

                if (secilen.DepoId.HasValue)
                    cmbDepo.SelectedValue = secilen.DepoId.Value;

                
            }
        }
        // Public method to load stok data from outside the class
        public void LoadStokData(StokModel stok)
        {
            if (stok == null) return;

            // Load all the stok information to the form fields
            txtStokKodu.Text = stok.StokKodu;
            txtStokAdi.Text = stok.StokAdi;
            SetComboBoxValue(cmbBirim, stok.Birim);
            txtAgirlik.Text = stok.Agirlik.ToString("0.##");
            txtProtein.Text = stok.Protein.ToString("0.##");
            txtEnerji.Text = stok.Enerji.ToString("0.##");
            txtNemOrani.Text = stok.Nem.ToString("0.##");
            txtBarkod.Text = stok.Barkod;
            SetComboBoxValue(cmbYemOzelligi, stok.YemOzelligi);
            txtAciklama.Text = stok.Aciklama;
            SetComboBoxValue(cmbMensei, stok.Mensei);

            // Fiyat verileri
            txtAlisFiyat.Text = stok.AlisFiyat?.ToString("0.##");
            txtAlisFiyat2.Text = stok.AlisFiyat2?.ToString("0.##");
            txtAlisFiyat3.Text = stok.AlisFiyat3?.ToString("0.##");
            txtAlisFiyat4.Text = stok.AlisFiyat4?.ToString("0.##");
            txtAlisFiyat5.Text = stok.AlisFiyat5?.ToString("0.##");
            txtKDV.Text = stok.KdvOrani?.ToString("0.##");

            if (stok.ListeTarihi.HasValue)
                dpListeTarihi.SelectedDate = stok.ListeTarihi.Value;

            // Depo bilgileri
            if (stok.IslemTarihi.HasValue)
                dpIslemTarihi.SelectedDate = stok.IslemTarihi.Value;

            if (stok.DepoId.HasValue)
                cmbDepo.SelectedValue = stok.DepoId.Value;

            // Dosya yolu
            txtDosyaYolu.Text = stok.DosyaYolu;

            // Lock the stok code so it won't be edited
            txtStokKodu.IsReadOnly = true;
        }

        private void btnTedarikciEkle_Click(object sender, RoutedEventArgs e)
        {
            TedarikciEkleWindow ekleWindow = new TedarikciEkleWindow();
            ekleWindow.Owner = this;
            ekleWindow.ShowDialog();

            if (ekleWindow.KayitBasarili)
            {
                // Listeyi yeniden doldur
                TedarikciComboDoldur();

                // Yeni ekleneni seçili yap
                cmbTedarikci.SelectedItem = ekleWindow.YeniTedarikciAdi;
            }
        }
        private void TedarikciComboDoldur()
        {
            cmbTedarikci.Items.Clear();

            try
            {
                string connStr = ConfigurationHelper.GetConnectionString("db");
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT TEDARIKCIADI FROM STOKSABITTED ORDER BY TEDARIKCIADI";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        cmbTedarikci.Items.Add(reader["TEDARIKCIADI"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tedarikçi listesi alınamadı: " + ex.Message);
            }
        }




    }
}