using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;

namespace UniDomNWC
{
    public partial class AutoLoginDom : Form
    {
        string account, password;
        bool isFirstTime = true;
        private enum LOGIN_STATUS
        {
            SUCCESS,
            WRONG_PASSWORD,
            ACCOUNT_NOT_AVAILABLE,
            ACCOUNT_EXPIRED,
            PC_LIMIT,
            LOGIN_NOT_AVAILABLE,
            UNKNOW_ERROR,

        }

        private enum PROCESS_STATE
        {
            CONNECTING,
            LOGIN,
        }

        public AutoLoginDom(string account, string password)
        {
            InitializeComponent();

            // Setting for web browser
            webBrowser1.ScriptErrorsSuppressed = true;

            this.account = account;
            this.password = password;
        }

        private void AutoLoginDom_Load(object sender, EventArgs e)
        {
            AnimateTitle(PROCESS_STATE.CONNECTING);
        }

        private void AnimateTitle(PROCESS_STATE state)
        {

            switch (state)
            {
                case PROCESS_STATE.CONNECTING:
                    this.Text = "Đang kết nối...";
                    break;
                case PROCESS_STATE.LOGIN:
                    this.Text = "Đang đăng nhập...";
                    break;
                default:
                    break;
            }
        }

        private void login()
        {
            webBrowser1.Document.GetElementById("auth_user").SetAttribute("value", account);
            webBrowser1.Document.GetElementById("auth_pass").SetAttribute("value", password);
            webBrowser1.Document.GetElementById("accept").InvokeMember("click");
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            AnimateTitle(PROCESS_STATE.LOGIN);
            switch (CheckConnectionStatus())
            {
                case LOGIN_STATUS.SUCCESS:
                    Form1.setGoogleDns(true);
                    MessageBox.Show("Đăng nhập thành công! (Đã có mạng Internet và sử dụng Google DNS)", "Thành công!");
                    this.Close();
                    break;
                case LOGIN_STATUS.WRONG_PASSWORD:
                    MessageBox.Show("Sai mật khẩu!", "Lỗi!");
                    this.Close();
                    break;
                case LOGIN_STATUS.ACCOUNT_NOT_AVAILABLE:
                    MessageBox.Show("Tài khoản không tồn tại!", "Lỗi!");
                    this.Close();
                    break;
                case LOGIN_STATUS.ACCOUNT_EXPIRED:
                    MessageBox.Show("Tài khoản hết hạn sử dụng!", "Lỗi!");
                    this.Close();
                    break;
                case LOGIN_STATUS.PC_LIMIT:
                    DialogResult result = MessageBox.Show("Số lượng PC vượt quá giới hạn!\nBạn có muốn thử lại không?.", "Lỗi!", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        login();
                    }
                    else
                        this.Close();
                    break;
                case LOGIN_STATUS.UNKNOW_ERROR:

                    if (isFirstTime)
                    {
                        isFirstTime = false;
                        login();
                        break;
                    }

                    DialogResult result2 = MessageBox.Show("Có lỗi xảy ra! Bạn có muốn thử lại không?.", "Lỗi!", MessageBoxButtons.YesNo);
                    if (result2 == DialogResult.Yes)
                    {
                        login();
                    }
                    else
                        this.Close();
                    break;
                case LOGIN_STATUS.LOGIN_NOT_AVAILABLE:
                    MessageBox.Show("Có lỗi xảy ra. Không thể đăng nhập!", "Lỗi!");
                    this.Close();
                    break;
                default:
                    break;
            }
        }


        private LOGIN_STATUS CheckConnectionStatus()
        {
            string content = webBrowser1.DocumentText;

            if (CheckForInternetConnection()) return LOGIN_STATUS.SUCCESS;

            if (isFptLoginPage())
            {
                if (content.Contains("Khong dung mat khau")) return LOGIN_STATUS.WRONG_PASSWORD;
                if (content.Contains("khong ton tai")) return LOGIN_STATUS.ACCOUNT_NOT_AVAILABLE;
                if (content.Contains("vuot qua gioi han")) return LOGIN_STATUS.PC_LIMIT;
                if (content.Contains("het han")) return LOGIN_STATUS.ACCOUNT_EXPIRED;
                return LOGIN_STATUS.UNKNOW_ERROR;
            }
            else return LOGIN_STATUS.LOGIN_NOT_AVAILABLE;
        }

        private bool isFptLoginPage()
        {
            return webBrowser1.Document.GetElementById("auth_user").TagName == "INPUT" &&
                webBrowser1.Document.GetElementById("auth_pass").TagName == "INPUT" &&
                webBrowser1.Document.GetElementById("accept").TagName == "INPUT";
        }

        private bool CheckForInternetConnection()
        {
            string host = "google.com";
            bool result = false;
            Ping p = new Ping();
            try
            {
                PingReply reply = p.Send(host, 3000);
                if (reply.Status == IPStatus.Success)
                    return true;
            }
            catch { }
            return result;
        }

        //public static bool CheckForInternetConnection()
        //{
        //    try
        //    {
        //        using (var client = new WebClient())
        //        using (var stream = client.OpenRead("http://www.google.com"))
        //        {
        //            return true;
        //        }
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}


    }
}
