using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;

namespace OtelOtomasyonu
{
    public partial class Form1 : Form
    {
        int secilenId = 0;

        SqlConnection baglanti = new SqlConnection("Data Source=alikalyon\\SQLEXPRESS;Initial Catalog=HotelDb;Integrated Security=True");
        public Form1()
        {
            InitializeComponent();
        }
        private void VerileriGoster()
        {

            baglanti.Open();


            SqlCommand komut = new SqlCommand("SELECT * FROM Rezervasyonlar", baglanti);

            SqlDataAdapter da = new SqlDataAdapter(komut);
            DataTable dt = new DataTable();
            da.Fill(dt);


            dataGridView1.DataSource = dt;


            baglanti.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            VerileriGoster();
            
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            
            if (txtAd.Text == "" || txtSoyad.Text == "" || txtOdaNo.Text == "")
            {
                MessageBox.Show("Lütfen tüm alanları doldurunuz!", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            
            DateTime girisTarihi = dtpGiris.Value.Date;
            DateTime cikisTarihi = dtpCikis.Value.Date;

            if (cikisTarihi <= girisTarihi)
            {
                MessageBox.Show("Çıkış tarihi, giriş tarihinden sonra olmalıdır!", "Tarih Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            
            TimeSpan sure = cikisTarihi - girisTarihi; 
            int gunSayisi = (int)sure.TotalDays;       
            int gecelikUcret = 1500;                   
            int toplamTutar = gunSayisi * gecelikUcret; 

            
            long kontrolSayi;
            if (!long.TryParse(txtOdaNo.Text, out kontrolSayi))
            {
                MessageBox.Show("Lütfen geçerli bir oda numarası giriniz! (Sadece rakam)", "Format Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            
            try
            {
                baglanti.Open();

                
                SqlCommand sorgu = new SqlCommand("SELECT COUNT(*) FROM Rezervasyonlar WHERE OdaNumarası = @odano", baglanti);
                sorgu.Parameters.AddWithValue("@odano", txtOdaNo.Text);
                int odaSayisi = (int)sorgu.ExecuteScalar();

                if (odaSayisi > 0)
                {
                    MessageBox.Show("Oda " + txtOdaNo.Text + " maalesef DOLU! Lütfen başka oda seçiniz.", "Çifte Rezervasyon Hatası", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    baglanti.Close();
                    return;
                }

               
                SqlCommand komut = new SqlCommand("INSERT INTO Rezervasyonlar (Ad, Soyad, OdaNumarası, GirişTarihi, ÇıkışTarihi) VALUES (@ad, @soyad, @odano, @giris, @cikis)", baglanti);

                komut.Parameters.AddWithValue("@ad", txtAd.Text);
                komut.Parameters.AddWithValue("@soyad", txtSoyad.Text);
                komut.Parameters.AddWithValue("@odano", txtOdaNo.Text);
                komut.Parameters.AddWithValue("@giris", girisTarihi);
                komut.Parameters.AddWithValue("@cikis", cikisTarihi);

                komut.ExecuteNonQuery();
                baglanti.Close();

                VerileriGoster();

                
                txtAd.Clear();
                txtSoyad.Clear();
                txtOdaNo.Clear();

                
                MessageBox.Show("Transilvanya Oteli'ne hoş geldiniz. Canavarca rahat bir konaklama sizi bekliyor.!”\n\n" +
                                "Konaklama Süresi: " + gunSayisi + " Gün\n" +
                                "Toplam Tutar: " + toplamTutar.ToString("N0") + " TL\n\n" + 
                                "İyi tatiller dileriz!",
                                "İşlem Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception hata)
            {
                if (baglanti.State == System.Data.ConnectionState.Open) baglanti.Close();
                MessageBox.Show("Hata: " + hata.Message);
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
           
            if (secilenId == 0)
            {
                MessageBox.Show("Lütfen silmek istediğiniz satıra tıklayın.", "Seçim Yapılmadı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            
            DialogResult cevap = MessageBox.Show("Rezervasyon ID: " + secilenId + "\nBu kaydı silmek istediğinize emin misiniz?",
                                                 "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (cevap == DialogResult.Yes)
            {
                try
                {
                    baglanti.Open();

                    
                    SqlCommand komut = new SqlCommand("DELETE FROM Rezervasyonlar WHERE RezervasyonID = @id", baglanti);

                    komut.Parameters.AddWithValue("@id", secilenId);

                    komut.ExecuteNonQuery(); 

                    baglanti.Close();

                    
                    MessageBox.Show("Kayıt başarıyla silindi!");

                    VerileriGoster(); 

                    
                    secilenId = 0;
                    txtAd.Clear();
                    txtSoyad.Clear();
                    txtOdaNo.Clear();
                }
                catch (Exception hata)
                {
                    baglanti.Close();
                    MessageBox.Show("Bir hata oluştu: " + hata.Message);
                }
            }
        }
        private void txtAra_TextChanged(object sender, EventArgs e)
        {

            baglanti.Open();

            SqlCommand komut = new SqlCommand("SELECT * FROM Rezervasyonlar WHERE Ad LIKE '%" + txtAra.Text + "%'", baglanti);

            SqlDataAdapter da = new SqlDataAdapter(komut);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dataGridView1.DataSource = dt;

            baglanti.Close();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            
            if (e.RowIndex < 0) return;

            
            DataGridViewRow satir = dataGridView1.Rows[e.RowIndex];

           
            try
            {
                secilenId = Convert.ToInt32(satir.Cells[0].Value);
            }
            catch
            {
                secilenId = 0; // Hata olursa 0 yap
            }

           
            txtAd.Text = satir.Cells[1].Value.ToString();    
            txtSoyad.Text = satir.Cells[2].Value.ToString();  
            txtOdaNo.Text = satir.Cells[3].Value.ToString(); 

           
            this.Text = "Seçilen ID: " + secilenId.ToString();
        }

        private void btnGuncelle_Click(object sender, EventArgs e)
        {
            if (secilenId == 0)
            {
                MessageBox.Show("Lütfen listeden güncellenecek kaydı seçin.");
                return;
            }

            try
            {
                baglanti.Open();

                
                string sorgu = "UPDATE Rezervasyonlar SET Ad=@ad, Soyad=@soyad, OdaNumarası=@oda, GirişTarihi=@giris, ÇıkışTarihi=@cikis WHERE RezervasyonID=@id";

                SqlCommand komut = new SqlCommand(sorgu, baglanti);

                komut.Parameters.AddWithValue("@ad", txtAd.Text);
                komut.Parameters.AddWithValue("@soyad", txtSoyad.Text);
                komut.Parameters.AddWithValue("@oda", txtOdaNo.Text);
                komut.Parameters.AddWithValue("@giris", dtpGiris.Value);
                komut.Parameters.AddWithValue("@cikis", dtpCikis.Value);
                komut.Parameters.AddWithValue("@id", secilenId); 

                komut.ExecuteNonQuery();
                baglanti.Close();

                MessageBox.Show("Kayıt başarıyla güncellendi!");
                VerileriGoster(); 

                
                secilenId = 0;
                txtAd.Clear(); txtSoyad.Clear(); txtOdaNo.Clear();
            }
            catch (Exception hata)
            {
                baglanti.Close();
                MessageBox.Show("Güncelleme Hatası: " + hata.Message);
            }
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            {
                
                txtAd.Clear();
                txtSoyad.Clear();
                txtOdaNo.Clear();

                
                dtpGiris.Value = DateTime.Now;
                dtpCikis.Value = DateTime.Now;

                
                secilenId = 0;

                MessageBox.Show("Form temizlendi, yeni kayıt ekleyebilirsiniz.", "Hazır", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_Click(object sender, EventArgs e)
        {

        }

        private void txtSoyad_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtAd_TextChanged(object sender, EventArgs e)
        {

        }
    }
   }





