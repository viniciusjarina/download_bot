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

			long start = 0;
			long end   = -1;
			int step   = 1;
			
			bool isDate = DateTime.TryParse (range [0], out dateStart);
			
			if (isDate && hasEnd) {
				isDate = DateTime.TryParse (range [1], out dateEnd);
			}
			
			if (isDate) {
				if (!hasStep || !TimeSpan.TryParse (range [2], out timeStep))
					timeStep = TimeSpan.FromDays (1.0);
			} else {
				if (!long.TryParse (range [0], out start)) {
					Console.WriteLine ("Invalid range format {0}, use numbers or date", range [0]);
					return;
				}

				if (hasEnd)
					hasEnd = long.TryParse (range [1], out end);

				if (!hasStep || int.TryParse (range [2], out step))
				    step = 1;
			}

			string dir;

			if (args.Length > 2)
				dir = args [2];
			else
				dir = Directory.GetCurrentDirectory ();

			var mc = new Downloader (dir);

			if (isDate)
				mc.DownloadFilesInDateRange (url, dateStart, dateEnd, timeStep, hasEnd);
			else
				mc.DownloadFilesInRange (url, start, end, step, hasEnd);	
		}

		string tempDir;

		public Downloader(string dir)
		{
			tempDir = dir;
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

				Console.WriteLine ("Downloading file: {0} ...", uri);

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

		
		public void DownloadFilesInRange(string pattern, long start, long end, int step, bool hasEnd)
		{
			if (start > end)
				throw new ArgumentException ("Start cannot be gretar than end");

			long delta = end > start ? end - start : 1;
			for (long i = start; i <= end; i += step)
			{
				string url = string.Format(pattern, i);

				double progress = (100.0 * (i - start)) / delta;
				Console.WriteLine ("{0:0.0}%", progress);
				
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
