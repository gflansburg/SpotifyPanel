using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Modules;

namespace SpotifyPanel
{
	public partial class Form1 : Form
	{
		public static Form1 Instance;
		private Token _token = null;
		private bool isAuthenticating = false;
		private AuthorizationCodeAuth authorizationCodeAuth;
		protected CancellationTokenSource serverSource;
		private WebServer webServer;

		public Token Token
		{
			get
			{
				return _token;
			}
			set
			{
				Properties.Settings.Default.Token = _token = value;
				Properties.Settings.Default.Save();
				if (_token != null)
				{
					if (_token.IsExpired())
					{
						RefreshToken();
					}
				}
			}
		}

		private void Authenticate()
		{
			if ((Token == null || Token.IsExpired()) && !isAuthenticating && authorizationCodeAuth != null && webView21.CoreWebView2 != null)
			{
				isAuthenticating = true;
				authorizationCodeAuth.Start();
				authorizationCodeAuth.OpenBrowser();
			}
		}

		private async void RefreshToken()
		{
			if (authorizationCodeAuth != null)
			{
				if (Token != null && Token.IsExpired() && !String.IsNullOrEmpty(Token.RefreshToken) && !isAuthenticating)
				{
					isAuthenticating = true;
					Token = await authorizationCodeAuth.RefreshToken(Token.RefreshToken);
					isAuthenticating = false;
				}
				else if (Token == null || Token.IsExpired())
				{
					Authenticate();
				}
			}
		}

		private async void AuthOnAuthReceived(object sender, AuthorizationCode payload)
		{
			authorizationCodeAuth.Stop();
			Token = await authorizationCodeAuth.ExchangeCode(payload.Code);
			isAuthenticating = false;
		}

		public Form1()
		{
			Instance = this;
			InitializeComponent();
		}

        private void timer2_Tick(object sender, EventArgs e)
        {
			lblStatus.Text = (IsConfigured ? (IsAuthorized ? "Authorized" : "Unauthorized") : "Unconfigured");
			lblStatus.ForeColor = (IsAuthorized ? System.Drawing.Color.Green : System.Drawing.Color.Red);
			lblExpires.Visible = IsAuthorized;
			lblDate.Visible = IsAuthorized;
			webView21.Visible = isAuthenticating;
			if (!isAuthenticating)
			{
				if (Token == null)
				{
					Authenticate();
				}
				else if (Token.IsExpired())
				{
					RefreshToken();
				}
				else
				{
					TimeSpan t = (Token.CreateDate.Add(TimeSpan.FromSeconds(Token.ExpiresIn)) - DateTime.Now);
					lblDate.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
				}
			}
		}

		private void ConfigureSpotifyAuthentication()
        {
			if (IsConfigured)
			{
				if (authorizationCodeAuth != null)
				{
					authorizationCodeAuth.Stop(0);
					authorizationCodeAuth = null;
				}
				authorizationCodeAuth = new AuthorizationCodeAuth(Properties.Settings.Default.ClientID, Properties.Settings.Default.SecretID, "http://localhost:52000", "http://localhost:52000", Scope.PlaylistReadPrivate | Scope.PlaylistReadCollaborative | Scope.UserReadCurrentlyPlaying | Scope.UserReadPlaybackState | Scope.UserModifyPlaybackState | Scope.UserLibraryRead | Scope.UserLibraryModify);
				authorizationCodeAuth.Browser = webView21;
				authorizationCodeAuth.AuthReceived += AuthOnAuthReceived;
				try
				{
					serverSource = new CancellationTokenSource();
					webServer = WebServer.Create(string.Format("http://127.0.0.1:{0}", 5150));
					webServer.RegisterModule(new WebApiModule());
					webServer.Module<WebApiModule>().RegisterController<SpotifyPanelController>();
					webServer.RegisterModule(new ResourceFilesModule(Assembly.GetExecutingAssembly(), GetEmbededResourcePath));
#pragma warning disable 4014
					webServer.RunAsync(serverSource.Token);
#pragma warning restore 4014
				}
				catch (Exception)
                {
					lblStatus.Text = "Port 5150 already in use!";
					lblStatus.ForeColor = System.Drawing.Color.Red;
					ShowWindow();
					return;
				}
			}
			timer2.Enabled = IsConfigured;
			lblStatus.Text = (IsConfigured ? (IsAuthorized ? "Authorized" : "Unauthorized") : "Unconfigured");
			lblStatus.ForeColor = (IsAuthorized ? System.Drawing.Color.Green : System.Drawing.Color.Red);
			label2.Visible = IsConfigured;
			pictureBox2.Visible = IsConfigured;
			lblExpires.Visible = IsAuthorized;
			lblDate.Visible = IsAuthorized;
			if(Token != null)
            {
				TimeSpan t = (Token.CreateDate.Add(TimeSpan.FromSeconds(Token.ExpiresIn)) - DateTime.Now);
				lblDate.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
			}
		}

		public static string ReplaceCSS(string html, string filename)
		{
			string text = GetEmbededResource(filename);
			return html.Replace(string.Format("[{0}]", filename), string.Format("<style type=\"text/css\">\r\n{0}</style>\r\n", text));
		}

		public static string ReplaceJS(string html, string filename)
        {
			string text = GetEmbededResource(filename);
			return html.Replace(string.Format("[{0}]", filename), string.Format("<script type=\"text/javascript\">\r\n{0}</script>\r\n", text));
		}

        private void btnSettings_Click(object sender, EventArgs e)
        {
			tbClientID.Text = Properties.Settings.Default.ClientID;
			tbSecretID.Text = Properties.Settings.Default.SecretID;
			pnlMain.Visible = false;
			pnlSettings.Visible = true;
			tbClientID.Focus();
        }

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == NativeMethods.WM_SYSCOMMAND)
			{
				if (m.WParam.ToInt32() == NativeMethods.SC_MINIMIZE)
				{
					// Caption bar minimize button
					m.Result = IntPtr.Zero;
					ShowInTaskbar = false;
					Visible = false;
					return;
				}
				else if (m.WParam.ToInt32() == NativeMethods.SC_CLOSE)
				{
					// Caption bar close button
					m.Result = IntPtr.Zero;
					ShowInTaskbar = false;
					Visible = false;
					return;
				}
			}
			base.WndProc(ref m);
		}

        private void label2_Click(object sender, EventArgs e)
        {
			AuthUtil.OpenBrowser(label2.Text);
		}

		protected bool IsConfigured
        {
			get
            {
				return !string.IsNullOrEmpty(Properties.Settings.Default.ClientID) && !string.IsNullOrEmpty(Properties.Settings.Default.SecretID);
			}
        }

		protected bool IsAuthorized
        {
			get
			{
				return Token != null && !string.IsNullOrEmpty(Token.AccessToken) && !Token.IsExpired();
			}
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
			if (!tbClientID.Text.Equals(Properties.Settings.Default.ClientID ?? "") || !tbSecretID.Text.Equals(Properties.Settings.Default.SecretID))
			{
				timer2.Enabled = false;
				Token = null;
				isAuthenticating = false;
				Properties.Settings.Default.ClientID = tbClientID.Text;
				Properties.Settings.Default.SecretID = tbSecretID.Text;
				Properties.Settings.Default.Save();
			}
			pnlSettings.Visible = false;
			pnlMain.Visible = true;
			ConfigureSpotifyAuthentication();
        }

        private void btnCancelSettings_Click(object sender, EventArgs e)
        {
			pnlSettings.Visible = false;
			pnlMain.Visible = true;
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
			ShowWindow();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
			Close();
		}

		private void ShowWindow()
		{
			WindowState = FormWindowState.Normal;
			ShowInTaskbar = true;
			this.Show();
			this.BringToFront();
			this.Focus();
			this.Activate();
		}

		private void Form1_Load(object sender, EventArgs e)
        {
			_token = Properties.Settings.Default.Token;
		}

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
			ShowWindow();
        }

		static public string GetEmbededResourcePath
		{
			get
			{
				Assembly thisAssembly = Assembly.GetExecutingAssembly();
				return thisAssembly.GetName().Name + ".www";
			}
		}

		public static string GetEmbededResource(string name)
        {
			Assembly thisAssembly = Assembly.GetExecutingAssembly();
			string appName = thisAssembly.GetName().Name;
			using (Stream stream = thisAssembly.GetManifestResourceStream(appName + ".www." + name))
			{
				using (TextReader reader = new StreamReader(stream))
                {
					return reader.ReadToEnd();
                }
			}
		}

        private void lblCreateKeys_Click(object sender, EventArgs e)
        {
			AuthUtil.OpenBrowser("https://developer.spotify.com/dashboard/");
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
			if (IsConfigured)
			{
				Visible = false;
				ShowInTaskbar = false;
			}
			ConfigureSpotifyAuthentication();
		}
	}
}
