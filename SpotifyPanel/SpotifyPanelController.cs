using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Constants;
using Unosquare.Labs.EmbedIO.Modules;

namespace SpotifyPanel
{
	internal class SpotifyPanelController : WebApiController
	{
		[WebApiHandler(HttpVerbs.Get, "/")]
		public Task<bool> GetEmpty()
		{
			string html = GetDefault();
			return this.StringResponseAsync(html, "text/html");
		}

		[WebApiHandler(HttpVerbs.Get, "/index.html")]
		public Task<bool> GetIndex()
		{
			string html = GetDefault();
			return this.StringResponseAsync(html, "text/html");
		}

		private string GetDefault()
        {
			string html = Form1.GetEmbededResource("index.html");
			html = Form1.ReplaceCSS(html, "styles.css");
			html = Form1.ReplaceJS(html, "helpers.js");
			html = Form1.ReplaceJS(html, "jquery.min.js");
			html = Form1.ReplaceJS(html, "page-handler.js");
			html = Form1.ReplaceJS(html, "vibrant.min.js");
			html = Form1.ReplaceJS(html, "spotify-web-api.js");
			html = Form1.ReplaceJS(html, "spotify-handler.js");
			html = Form1.ReplaceJS(html, "progressbar.js");
			html = html.Replace("[Access-Token]", string.Format("{0}", Form1.Instance.Token != null ? Form1.Instance.Token.AccessToken : string.Empty));
			html = html.Replace("[Expires-In]", string.Format("{0}", Form1.Instance.Token != null ? Form1.Instance.Token.CreateDate.Add(TimeSpan.FromSeconds(Form1.Instance.Token.ExpiresIn)).ToString() : DateTime.Now.ToString()));
			return html;
		}

		public SpotifyPanelController(IHttpContext context) : base(context)
		{
		}
	}
}
