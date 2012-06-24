using System;
using System.Net;
using System.ComponentModel;
using System.IO;

namespace download_bot
{
	class Downloader
	{
		public static bool UrlExists(Uri url)
		{
			try 
			{
				HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create (url);
				request.AllowAutoRedirect = false;
				HttpWebResponse response = (HttpWebResponse)request.GetResponse ();
				bool isOK = response.StatusCode == HttpStatusCode.OK;
				response.Close ();
				return isOK;
			}
    		catch(WebException) {
			}
			return false;    
		}

		public static void Main (string[] args)
		{
			if (args.Length < 2) {
				Console.WriteLine ("Usage: download_bot url start[;end[;step]] [output dir]");
				return;
			}
			
			string url = args [0];
			string [] range = args [1].Split (';');
			
			bool hasEnd  = range.Length > 1;
			bool hasStep = range.Length > 2;
			
			DateTime dateStart;
			DateTime dateEnd;

			TimeSpan timeStep = TimeSpan.FromDays (1.0);

			int start = 0;
			int end  = -1;
			int step = 1;
			
			bool isDate = DateTime.TryParse (range [0], out dateStart);
			
			if (isDate && hasEnd) {
				isDate = DateTime.TryParse (range [1], out dateEnd);
			}
			
			if (isDate) {
				if (!hasStep || !TimeSpan.TryParse (range [2], out timeStep))
					timeStep = TimeSpan.FromDays (1.0);
			} else {
				if (!int.TryParse (range [0], out start)) {
					Console.WriteLine ("Invalid range format {0}, use numbers or date", range [0]);
					return;
				}

				if (hasEnd)
					hasEnd = int.TryParse (range [1], out end);

				if (!hasStep || int.TryParse (range [2], out step))
				    step = 1;
			}

			var mc = new Downloader ();

			if (isDate)
				mc.DownloadFilesInDateRange (url, dateStart, dateEnd, timeStep, hasEnd);
			else
				mc.DownloadFilesInRange (url, start, end, step, hasEnd);	
		}

		string tempDir;

		public Downloader()
		{
			tempDir = Directory.GetCurrentDirectory ();
		}
		
		public bool DownloadFile (string url)
		{
			using(WebClient client = new WebClient ())
			{
				Uri uri;

				if (!Uri.TryCreate (url, UriKind.Absolute, out uri))
					return false;

				//downloads
				string filePath     = Path.Combine (tempDir, "." + uri.LocalPath);
				string fullLocalDir = Path.GetDirectoryName (filePath);
				
				if(!Directory.Exists (fullLocalDir))
					Directory.CreateDirectory (fullLocalDir);

				string path = Path.Combine(tempDir , "." + uri.LocalPath);

				if (File.Exists (path))
					return true;

				if (!UrlExists (uri))
					return false;

				try
				{
					client.DownloadFile (uri, path);
				}
				catch (WebException ex)
				{
				    if (ex.Status == WebExceptionStatus.ProtocolError) {
						if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
							return false;
					}
				}
			}
			return true;
		}

		
		public void DownloadFilesInRange(string pattern, int start, int end, int step, bool hasEnd)
		{
			if (start > end)
				throw new ArgumentException ("Start cannot be gretar than end");
			
			for (int i = start; i <= end; i += step)
			{
				string url = string.Format(pattern, i);
				
				bool fail = DownloadFile(url);

				if (!hasEnd && fail)
						break;
			}
		}
		
		public void DownloadFilesInDateRange(string pattern, DateTime start, DateTime end, TimeSpan step, bool hasEnd)
		{
			if (start > end)
				throw new ArgumentException ("Start cannot be gretar than end");
			
			for (DateTime i = start; i <= end; i += step)
			{
				string url = string.Format(pattern, i);
				
				bool fail = DownloadFile(url);

				if (!hasEnd && fail)
						break;
			}
		}
	}
}
