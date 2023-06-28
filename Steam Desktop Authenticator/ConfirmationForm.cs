using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using SteamAuth;
using System.Net;

namespace Steam_Desktop_Authenticator
{
    public partial class ConfirmationForm : Form
    {
        private string steamCookies;
        private SteamGuardAccount steamAccount;
        private string tradeID;

        public ConfirmationForm(SteamGuardAccount steamAccount)
        {
            InitializeComponent();
            this.steamAccount = steamAccount;
            this.Text = String.Format("Offer list - {0}", steamAccount.AccountName);
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            ReloadTrades();
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        List<Confirmation> Confirmations = new List<Confirmation>();

        public void ReloadTrades()
        {
            dataGridView1.Rows.Clear();
            Confirmations.Clear();

            try
            {
                var confs = steamAccount.FetchConfirmations();

                if (confs != null)
                {
                    foreach (var conf in confs)
                    {
                        var summary = "";

                        if (conf.summary != null)
                            conf.summary.ForEach(x => summary += x + Environment.NewLine);

                        Bitmap avatar = new Bitmap(10, 10);

                        try
                        {
                            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(conf.icon);
                            myRequest.Method = "GET";
                            using (HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse())
                            {
                                var img = Bitmap.FromStream(myResponse.GetResponseStream());
                                avatar = new System.Drawing.Bitmap(img, 100, 100);
                                myResponse.Close();
                            }
                        }
                        catch { }

                        dataGridView1.Rows.Add(conf.type_name, conf.headline, avatar, UnixTimeStampToDateTime(conf.creation_time).ToLocalTime(), summary, "Принять", "Отклонить");
                        Confirmations.Add(conf);
                    }



                    if (confs.Length > 0)
                    {
                        label1.Text = "";
                        label1.Visible = false;
                    }
                    else
                    {
                        label1.Text = "Trade list is empty";
                        label1.Visible = true;
                    }
                }
                else
                {
                    label1.Text = "Failed to get list, possibly due to too many requests.\r\n\r\nPlease try again later";
                    label1.Visible = true;
                }
            }
            catch (Exception ex)
            {
                label1.Text = "Error getting trades.\r\n\r\nTry updating, if that doesn't work use 'Login again'";
                label1.Visible = true;
                //MessageBox.Show("Ошибка получения трейдов. Попробуйте обновить, если это не помогает используйте 'Перезайти (Login again)'");
            }
        }

        private void ConfirmationForm_Load(object sender, EventArgs e)
        {
            dataGridView1.AllowUserToAddRows = false;
            //dataGridView1.RowHeightInfoNeeded += DataGridView1_RowHeightInfoNeeded;
            dataGridView1.RowTemplate.Height = 100;

            ReloadTrades();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 5)
                {
                    if (steamAccount.AcceptConfirmation(Confirmations[e.RowIndex]))
                    {
                        Confirmations.RemoveAt(e.RowIndex);
                        dataGridView1.Rows.RemoveAt(e.RowIndex);
                    }
                    else
                    {
                        MessageBox.Show("Trade acceptance error. Refresh and try again");
                    }
                }
                else if (e.ColumnIndex == 6)
                {
                    if (steamAccount.DenyConfirmation(Confirmations[e.RowIndex]))
                    {
                        Confirmations.RemoveAt(e.RowIndex);
                        dataGridView1.Rows.RemoveAt(e.RowIndex);
                    }
                    else
                    {
                        MessageBox.Show("Trade rejection error. Refresh and try again");
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Click handling error"); }
        }
    }
}
