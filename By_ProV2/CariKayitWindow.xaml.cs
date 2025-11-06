using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;
using System.Windows.Input;

namespace By_ProV2
{
       public partial class CariKayitWindow : Window
       {
        public CariKayitWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dtKayitTarihi.SelectedDate = DateTime.Now;
        }
        private void btnKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCariKod.Text))
            {
                MessageBox.Show("Cari kodu boş bırakılamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCariKod.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCariAdi.Text))
            {
                MessageBox.Show("Cari adı boş bırakılamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCariAdi.Focus();
                return;
            }

            // İs kontoları parse etmek için değişkenler
            decimal isk1 = 0, isk2 = 0, isk3 = 0, isk4 = 0, isk5 = 0, isk6 = 0, isk7 = 0, isk8 = 0, isk9 = 0;

            // Virgül kabul eden tr-TR kültürüyle dönüştürme
            if (!TryParseDecimal(txtIsk1.Text, "İskonto 1", txtIsk1, out isk1)) return;
            if (!TryParseDecimal(txtIsk2.Text, "İskonto 2", txtIsk2, out isk2)) return;
            if (!TryParseDecimal(txtIsk3.Text, "İskonto 3", txtIsk3, out isk3)) return;
            if (!TryParseDecimal(txtIsk4.Text, "İskonto 4", txtIsk4, out isk4)) return;
            if (!TryParseDecimal(txtKKIsk1.Text, "Kredi Kartı İskonto 1", txtKKIsk1, out isk5)) return;
            if (!TryParseDecimal(txtKKIsk2.Text, "Kredi Kartı İskonto 2", txtKKIsk2, out isk6)) return;
            if (!TryParseDecimal(txtKKIsk3.Text, "Kredi Kartı İskonto 3", txtKKIsk3, out isk7)) return;
            if (!TryParseDecimal(txtKKIsk4.Text, "Kredi Kartı İskonto 4", txtKKIsk4, out isk8)) return;
            if (!TryParseDecimal(txtNakliyeIsk1.Text, "Nakliye İskonto", txtNakliyeIsk1, out isk9)) return;
            
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["db"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Aynı CARIKOD veritabanında var mı kontrolü
                    string kontrolSql = "SELECT COUNT(*) FROM CASABIT WHERE CARIKOD = @CARIKOD";
                    using (SqlCommand kontrolCmd = new SqlCommand(kontrolSql, conn))
                    {
                        kontrolCmd.Parameters.AddWithValue("@CARIKOD", txtCariKod.Text.Trim());

                        int kayitSayisi = (int)kontrolCmd.ExecuteScalar();

                        if (kayitSayisi > 0)
                        {
                            MessageBox.Show("Bu cari kod zaten kayıtlı. Lütfen farklı bir kod giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                            txtCariKod.Focus();
                            return;
                        }
                    }


                    int newID = GetNewID();

                    string sql = @"
                INSERT INTO CASABIT
                (ID, CARIKOD, CARIADI, ADRES, TELEFON, YETKILIKISI, BAGLICARIKOD,
                 VERGIDAIRESI, VERGINO, ISK1, ISK2, ISK3, ISK4, KKISK1, KKISK2, KKISK3, KKISK4, NAKISK,
                 PLAKA1, PLAKA2, PLAKA3, SOFORADSOYAD, KAYITTARIHI, CreatedBy, ModifiedBy, CreatedAt, ModifiedAt)
                VALUES
                (@ID, @CARIKOD, @CARIADI, @ADRES, @TELEFON, @YETKILIKISI, @BAGLICARIKOD,
                 @VERGIDAIRESI, @VERGINO, @ISK1, @ISK2, @ISK3, @ISK4, @KKISK1, @KKISK2, @KKISK3, @KKISK4, @NAKISK,
                 @PLAKA1, @PLAKA2, @PLAKA3, @SOFORADSOYAD, @KAYITTARIHI, @CreatedBy, @ModifiedBy, @CreatedAt, @ModifiedAt)";

                    SqlCommand cmd = new SqlCommand(sql, conn);

                    // Eğer bağlı cari kod boşsa, cari kodu kullan
                    string bagliCariKod = string.IsNullOrWhiteSpace(txtBagliCariKod.Text)
                        ? txtCariKod.Text.Trim()
                        : txtBagliCariKod.Text.Trim();

                    cmd.Parameters.AddWithValue("@ID", newID);
                    cmd.Parameters.AddWithValue("@CARIKOD", txtCariKod.Text);
                    cmd.Parameters.AddWithValue("@CARIADI", txtCariAdi.Text);
                    cmd.Parameters.AddWithValue("@ADRES", txtAdres.Text);
                    cmd.Parameters.AddWithValue("@TELEFON", txtTelefon.Text);
                    cmd.Parameters.AddWithValue("@YETKILIKISI", txtYetkili.Text);
                    cmd.Parameters.AddWithValue("@BAGLICARIKOD", bagliCariKod);
                    cmd.Parameters.AddWithValue("@VERGIDAIRESI", txtVergiDaire.Text);
                    cmd.Parameters.AddWithValue("@VERGINO", txtVergiNo.Text);

                    SqlParameter pIsk1 = new SqlParameter("@ISK1", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk1 };
                    SqlParameter pIsk2 = new SqlParameter("@ISK2", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk2 };
                    SqlParameter pIsk3 = new SqlParameter("@ISK3", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk3 };
                    SqlParameter pIsk4 = new SqlParameter("@ISK4", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk4 };
                    SqlParameter pIsk5 = new SqlParameter("@KKISK1", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk5 };
                    SqlParameter pIsk6 = new SqlParameter("@KKISK2", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk6 };
                    SqlParameter pIsk7 = new SqlParameter("@KKISK3", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk7 };
                    SqlParameter pIsk8 = new SqlParameter("@KKISK4", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk8 };
                    SqlParameter pIsk9 = new SqlParameter("@NAKISK", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk9 };

                    cmd.Parameters.Add(pIsk1);
                    cmd.Parameters.Add(pIsk2);
                    cmd.Parameters.Add(pIsk3);
                    cmd.Parameters.Add(pIsk4);
                    cmd.Parameters.Add(pIsk5);
                    cmd.Parameters.Add(pIsk6);
                    cmd.Parameters.Add(pIsk7);
                    cmd.Parameters.Add(pIsk8);
                    cmd.Parameters.Add(pIsk9);

                    cmd.Parameters.AddWithValue("@PLAKA1", txtPlaka1.Text);
                    cmd.Parameters.AddWithValue("@PLAKA2", txtPlaka2.Text);
                    cmd.Parameters.AddWithValue("@PLAKA3", txtPlaka3.Text);
                    cmd.Parameters.AddWithValue("@SOFORADSOYAD", txtSofor.Text);
                    cmd.Parameters.AddWithValue("@KAYITTARIHI", dtKayitTarihi.SelectedDate ?? DateTime.Now);
                    
                    // Add user tracking parameters for insert
                    var currentUserForInsert = App.AuthService?.CurrentUser;
                    if (currentUserForInsert != null)
                    {
                        cmd.Parameters.AddWithValue("@CreatedBy", currentUserForInsert.Id);
                        cmd.Parameters.AddWithValue("@ModifiedBy", currentUserForInsert.Id);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@CreatedBy", DBNull.Value);
                        cmd.Parameters.AddWithValue("@ModifiedBy", DBNull.Value);
                    }
                    cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ModifiedAt", DateTime.Now);

                    int sonuc = cmd.ExecuteNonQuery();

                    if (sonuc > 0)
                    {
                        MessageBox.Show("Cari kaydı başarıyla eklendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        TemizleForm();
                    }
                    else
                    {
                        MessageBox.Show("Kayıt eklenemedi!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu:\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetNewID()
        {
            int newID = 1;

            string connectionString = ConfigurationManager.ConnectionStrings["db"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT ISNULL(MAX(ID), 0) FROM CASABIT";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        newID = Convert.ToInt32(result) + 1;
                    }
                }
            }

            return newID;
        }


        private void TemizleForm()
        {
            txtCariKod.Clear();
            txtCariAdi.Clear();
            txtAdres.Clear();
            txtTelefon.Clear();
            txtYetkili.Clear();
            txtBagliCariKod.Clear();
            txtVergiDaire.Clear();
            txtVergiNo.Clear();
            txtIsk1.Clear();
            txtIsk2.Clear();
            txtIsk3.Clear();
            txtIsk4.Clear();
            txtKKIsk1.Clear();
            txtKKIsk2.Clear();
            txtKKIsk3.Clear();
            txtKKIsk4.Clear();
            txtNakliyeIsk1.Clear();
            txtPlaka1.Clear();
            txtPlaka2.Clear();
            txtPlaka3.Clear();
            txtSofor.Clear();
            dtKayitTarihi.SelectedDate = DateTime.Now;
            txtCariKod.IsReadOnly = false;

        }
        private void btnTemizle_Click(object sender, RoutedEventArgs e)
        {
            // Tüm TextBox'ları temizleyin
            txtCariKod.Text = "";
            txtCariAdi.Text = "";
            txtAdres.Text = "";
            txtTelefon.Text = "";
            txtYetkili.Text = "";
            txtIsk1.Text = "";
            txtIsk2.Text = "";
            txtIsk3.Text = "";
            txtKKIsk4.Text = "";
            txtKKIsk1.Text = "";
            txtKKIsk2.Text = "";
            txtKKIsk3.Text = "";
            txtKKIsk4.Text = "";
            txtNakliyeIsk1.Text = "";
            txtSofor.Text = "";
            txtPlaka1.Text = "";
            txtPlaka2.Text = "";
            txtPlaka3.Text = "";
            txtVergiDaire.Text = "";
            txtVergiNo.Text = "";
            txtBagliCariKod.Text = "";
            txtBagliCariAdiGoster.Text = "";
            dtKayitTarihi.SelectedDate = null;
        }

        private void btnCariKodAra_Click(object sender, RoutedEventArgs e)
        {
            CariListesiWindow listePenceresi = new CariListesiWindow();
            bool? sonuc = listePenceresi.ShowDialog();

            if (sonuc == true && listePenceresi.SecilenCari != null)
            {
                var secilen = listePenceresi.SecilenCari;

                txtCariKod.Text = secilen.CariKod;
                txtCariAdi.Text = secilen.CariAdi;
                // Cari kodu kilitle
                txtCariKod.IsReadOnly = true;
                // Eğer diğer bilgileri istiyorsan, burada ekstra sorgu yapman gerekir.
            }
        }


        private void btnGuncelle_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCariKod.Text))
            {
                MessageBox.Show("Güncellenecek cari kodu girilmelidir.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCariKod.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCariAdi.Text))
            {
                MessageBox.Show("Cari adı boş bırakılamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCariAdi.Focus();
                return;
            }

            // İskontoları parse et
            decimal isk1 = 0, isk2 = 0, isk3 = 0, isk4 = 0, isk5 = 0, isk6 = 0, isk7 = 0, isk8 = 0, isk9 = 0;

            if (!TryParseDecimal(txtIsk1.Text, "İskonto 1", txtIsk1, out isk1)) return;
            if (!TryParseDecimal(txtIsk2.Text, "İskonto 2", txtIsk2, out isk2)) return;
            if (!TryParseDecimal(txtIsk3.Text, "İskonto 3", txtIsk3, out isk3)) return;
            if (!TryParseDecimal(txtIsk4.Text, "İskonto 4", txtIsk4, out isk4)) return;
            if (!TryParseDecimal(txtKKIsk1.Text, "Kredi Kartı İskonto 1", txtKKIsk1, out isk5)) return;
            if (!TryParseDecimal(txtKKIsk2.Text, "Kredi Kartı İskonto 2", txtKKIsk2, out isk6)) return;
            if (!TryParseDecimal(txtKKIsk3.Text, "Kredi Kartı İskonto 3", txtKKIsk3, out isk7)) return;
            if (!TryParseDecimal(txtKKIsk4.Text, "Kredi Kartı İskonto 4", txtKKIsk4, out isk8)) return;
            if (!TryParseDecimal(txtNakliyeIsk1.Text, "Nakliye İskonto", txtNakliyeIsk1, out isk9)) return;

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["db"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = @"
            UPDATE CASABIT SET
                CARIADI = @CARIADI,
                ADRES = @ADRES,
                TELEFON = @TELEFON,
                YETKILIKISI = @YETKILIKISI,
                BAGLICARIKOD = @BAGLICARIKOD,
                VERGIDAIRESI = @VERGIDAIRESI,
                VERGINO = @VERGINO,
                ISK1 = @ISK1,
                ISK2 = @ISK2,
                ISK3 = @ISK3,
                ISK4 = @ISK4,
                KKISK1 = @KKISK1,
                KKISK2 = @KKISK2,
                KKISK3 = @KKISK3,
                KKISK4 = @KKISK4,
                NAKISK = @NAKISK,
                PLAKA1 = @PLAKA1,
                PLAKA2 = @PLAKA2,
                PLAKA3 = @PLAKA3,
                SOFORADSOYAD = @SOFORADSOYAD,
                KAYITTARIHI = @KAYITTARIHI,
                ModifiedBy = @ModifiedBy,
                ModifiedAt = @ModifiedAt
            WHERE CARIKOD = @CARIKOD";

                    SqlCommand cmd = new SqlCommand(sql, conn);

                    string bagliCariKod = string.IsNullOrWhiteSpace(txtBagliCariKod.Text)
                        ? txtCariKod.Text.Trim()
                        : txtBagliCariKod.Text.Trim();

                    cmd.Parameters.AddWithValue("@CARIKOD", txtCariKod.Text.Trim());
                    cmd.Parameters.AddWithValue("@CARIADI", txtCariAdi.Text.Trim());
                    cmd.Parameters.AddWithValue("@ADRES", txtAdres.Text.Trim());
                    cmd.Parameters.AddWithValue("@TELEFON", txtTelefon.Text.Trim());
                    cmd.Parameters.AddWithValue("@YETKILIKISI", txtYetkili.Text.Trim());
                    cmd.Parameters.AddWithValue("@BAGLICARIKOD", bagliCariKod);
                    cmd.Parameters.AddWithValue("@VERGIDAIRESI", txtVergiDaire.Text.Trim());
                    cmd.Parameters.AddWithValue("@VERGINO", txtVergiNo.Text.Trim());

                    cmd.Parameters.Add(new SqlParameter("@ISK1", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk1 });
                    cmd.Parameters.Add(new SqlParameter("@ISK2", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk2 });
                    cmd.Parameters.Add(new SqlParameter("@ISK3", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk3 });
                    cmd.Parameters.Add(new SqlParameter("@ISK4", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk4 });
                    cmd.Parameters.Add(new SqlParameter("@KKISK1", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk5 });
                    cmd.Parameters.Add(new SqlParameter("@KKISK2", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk6 });
                    cmd.Parameters.Add(new SqlParameter("@KKISK3", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk7 });
                    cmd.Parameters.Add(new SqlParameter("@KKISK4", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk8 });
                    cmd.Parameters.Add(new SqlParameter("@NAKISK", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = isk9 });

                    cmd.Parameters.AddWithValue("@PLAKA1", txtPlaka1.Text.Trim());
                    cmd.Parameters.AddWithValue("@PLAKA2", txtPlaka2.Text.Trim());
                    cmd.Parameters.AddWithValue("@PLAKA3", txtPlaka3.Text.Trim());
                    cmd.Parameters.AddWithValue("@SOFORADSOYAD", txtSofor.Text.Trim());
                    cmd.Parameters.AddWithValue("@KAYITTARIHI", dtKayitTarihi.SelectedDate ?? DateTime.Now);
                    
                    // Add user tracking parameters for update
                    var currentUserForUpdate = App.AuthService?.CurrentUser;
                    if (currentUserForUpdate != null)
                    {
                        cmd.Parameters.AddWithValue("@ModifiedBy", currentUserForUpdate.Id);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@ModifiedBy", DBNull.Value);
                    }
                    cmd.Parameters.AddWithValue("@ModifiedAt", DateTime.Now);

                    int affectedRows = cmd.ExecuteNonQuery();

                    if (affectedRows > 0)
                    {
                        MessageBox.Show("Cari kaydı başarıyla güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                        TemizleForm();
                    }
                    else
                    {
                        MessageBox.Show("Güncelleme işlemi başarısız.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata oluştu:\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool TryParseDecimal(string input, string fieldName, Control control, out decimal result)
        {
            result = 0;

            if (!string.IsNullOrWhiteSpace(input) &&
                !decimal.TryParse(input, NumberStyles.Any, new CultureInfo("tr-TR"), out result))
            {
                MessageBox.Show($"{fieldName} için geçerli bir sayı giriniz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                control.Focus();
                return false;
            }

            return true;
        }



        private void btnSil_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCariKod.Text))
            {
                MessageBox.Show("Silinecek cari kod girilmelidir.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult cevap = MessageBox.Show("Bu cariyi silmek istediğinize emin misiniz?", "Silme Onayı", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (cevap != MessageBoxResult.Yes)
                return;

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["db"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = "DELETE FROM CASABIT WHERE CARIKOD = @CARIKOD";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@CARIKOD", txtCariKod.Text);

                    int result = cmd.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Cari başarıyla silindi.", "Silindi", MessageBoxButton.OK, MessageBoxImage.Information);
                        TemizleForm();
                    }
                    else
                    {
                        MessageBox.Show("Silme işlemi başarısız.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnCariListe_Click(object sender, RoutedEventArgs e)
        {
            var listePenceresi = new CariListesiWindow();
            bool? sonuc = listePenceresi.ShowDialog();

            if (sonuc == true && listePenceresi.SecilenCari != null)
            {
                var secilen = listePenceresi.SecilenCari;

                // Seçilen carinin tüm bilgilerini forma doldur
                txtCariKod.Text = secilen.CariKod;
                txtCariAdi.Text = secilen.CariAdi;
                txtAdres.Text = secilen.Adres;
                txtTelefon.Text = secilen.Telefon;
                txtYetkili.Text = secilen.Yetkili;
                txtBagliCariKod.Text = secilen.BagliCariKod;
                txtVergiDaire.Text = secilen.VergiDairesi;
                txtVergiNo.Text = secilen.VergiNo;

                txtIsk1.Text = secilen.Isk1.ToString("0.##", new CultureInfo("tr-TR"));
                txtIsk2.Text = secilen.Isk2.ToString("0.##", new CultureInfo("tr-TR"));
                txtIsk3.Text = secilen.Isk3.ToString("0.##", new CultureInfo("tr-TR"));
                txtIsk4.Text = secilen.Isk4.ToString("0.##", new CultureInfo("tr-TR"));
                txtKKIsk1.Text = secilen.KKIsk1.ToString("0.##", new CultureInfo("tr-TR"));
                txtKKIsk2.Text = secilen.KKIsk2.ToString("0.##", new CultureInfo("tr-TR"));
                txtKKIsk3.Text = secilen.KKIsk3.ToString("0.##", new CultureInfo("tr-TR"));
                txtKKIsk4.Text = secilen.KKIsk4.ToString("0.##", new CultureInfo("tr-TR"));
                txtNakliyeIsk1.Text = secilen.NakliyeIskonto.ToString("0.##", new CultureInfo("tr-TR"));

                txtPlaka1.Text = secilen.Plaka1;
                txtPlaka2.Text = secilen.Plaka2;
                txtPlaka3.Text = secilen.Plaka3;
                txtSofor.Text = secilen.SoforAdSoyad;

                dtKayitTarihi.SelectedDate = secilen.KayitTarihi;

                // İstersen cari kodu düzenlenmesin diye kilitle
                txtCariKod.IsReadOnly = true;
            }
        }
        private void txtBagliCariKod_LostFocus(object sender, RoutedEventArgs e)
        {
            string bagliKod = txtBagliCariKod.Text.Trim();
            if (string.IsNullOrWhiteSpace(bagliKod))
            {
                txtBagliCariAdiGoster.Text = "";
                return;
            }

            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["db"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string sql = "SELECT CARIADI FROM CASABIT WHERE CARIKOD = @CARIKOD";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CARIKOD", bagliKod);

                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            txtBagliCariAdiGoster.Text = result.ToString();
                        }
                        else
                        {
                            txtBagliCariAdiGoster.Text = "❌ Cari bulunamadı";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bağlı cari adı getirilirken hata oluştu:\n" + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    textBox.SelectAll();
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

    }
}