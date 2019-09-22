using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SharpHydrus
{
	public static class Hydrus
	{
		private static string _API_KEY = "";

		public static string API_KEY
		{
			get { return _API_KEY; }
			set { _API_KEY = value; }
		}

		private static string _Server_Addr = "";

		public static string Server_Addr
		{
			get { return _Server_Addr; }
			set { _Server_Addr = value; }
		}

		/// <summary>
		/// Gets the current API version
		/// </summary>
		/// <returns>API_VERSION</returns>
		public static Task<API_VERSION> Get_Api_Version()
		{
			return Task.FromResult(JsonConvert.DeserializeObject<API_VERSION>(FetchJSON(Server_Addr + "/api_version").Result));
		}

		/// <summary>
		/// Register a new external program with the client. 
		/// This requires the 'add from api request' mini-dialog under services->review services to be open, otherwise it will 403.
		/// 
		/// Int array containing any of theese
		/// 0 - Import URLs
		/// 1 - Import Files
		/// 2 - Add Tags
		/// 3 - Search for Files
		/// 4 - Manage Pages
		/// 5 - Manage Cookies
		/// </summary>
		/// <param name="PermissionName">A descriptive name of the new permission</param>
		/// <param name="permissions">Array of permissions for the new key</param>
		/// <returns>ACCESS_KEY</returns>
		public static Task<ACCESS_KEY> Request_New_Permissions(string PermissionName, int[] Permissions)
		{
			string p = "[" + string.Join(",", Permissions) + "]";
			string url = $"/request_access_permissions?name={PermissionName}&basic_permissions={p}";
			return Task.FromResult(JsonConvert.DeserializeObject<ACCESS_KEY>(FetchJSON(_Server_Addr + url).Result));
		}

		/// <summary>
		/// Get a new session key.
		/// </summary>
		/// <returns>SESSION_KEY</returns>
		public static Task<SESSION_KEY> Get_Session_Key()
		{
			return Task.FromResult(JsonConvert.DeserializeObject<SESSION_KEY>(FetchJSON(_Server_Addr + "/session_key?Hydrus-Client-API-Access-Key=" + API_KEY).Result));
		}

		/// <summary>
		/// Check your access key is valid.
		/// </summary>
		/// <param name="key">Key to check, if none is set, it will check the API key, if that's not set, it will return null.</param>
		/// <returns>VERIFICATION</returns>
		public static Task<VERIFICATION> Verify_Access_Key(string key = null)
		{
			var k = key;
			if (k == null && API_KEY != null)
				k = API_KEY;
			else
			{
				Console.WriteLine("No key provided. No verification possible");
				return null;
			}

			return Task.FromResult(JsonConvert.DeserializeObject<VERIFICATION>(FetchJSON(_Server_Addr + "/verify_access_key?Hydrus-Client-API-Access-Key=" + k).Result));
		}


		/// <summary>
		/// Fetch the file tumbnail
		/// </summary>
		/// <param name="Filetype">Set it to use ID or file HASH</param>
		/// <param name="ab"></param>
		/// <returns>A thumbnail file</returns>
		public static Task<ResponseFile> Get_Files_Thumbnails(FILETYPE Filetype, string HashOrID)
		{
			try
			{
				var url = _Server_Addr + "/get_files/thumbnail?";
				switch (Filetype)
				{
					case FILETYPE.File_ID:
						url += "file_id=" + HashOrID;
						break;
					case FILETYPE.File_Hash:
						url += "hash=" + HashOrID;
						break;
					default:
						break;
				}

				var request = (HttpWebRequest)WebRequest.Create(url + "&Hydrus-Client-API-Access-Key=7508d2fcad4b2577bbbbb04308c4eb077c94d32948a54f277c884d5d4366d1a7");
				request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
				var f = request.GetResponse();

				string filetype = DetectImageType(f.GetResponseStream());


				var reg = new Regex("\"[^\"]*\"");
				var match = reg.Matches(f.Headers["Content-Disposition"])[0].Value.Replace("thumbnail", filetype).Trim('"');

				ResponseFile response = new ResponseFile
				{
					FileByteStream = f.GetResponseStream(),
					FileName = match
				};



				return Task.FromResult(response);

			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
				return null;
			}
		
		}


		/// <summary>
		/// Fetches JSON from a URL
		/// </summary>
		/// <param name="URL">Target URL</param>
		/// <returns>String</returns>
		public static Task<string> FetchJSON(string URL)
		{
			using (var client = new HttpClient())
			{
				try
				{
					var v = Task.FromResult(client.GetStringAsync(URL).Result);
					client.Dispose();
					return v;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
					client.Dispose();
					return null;
				}

			}
		}


		/// <summary>
		/// Uses mimetypes to detect file types
		/// </summary>
		/// <param name="stream">The relevant ocet stream to check</param>
		/// <returns>Known file type, or thumbnail if it has no clue</returns>
		public static string DetectImageType(Stream stream)
		{
			Dictionary<byte[], string> types = new Dictionary<byte[], string>
			{
				{ Encoding.ASCII.GetBytes("BM"), "bmp" }, // BMP 
				{ Encoding.ASCII.GetBytes("GIF"), "gif" }, // GIF 
				{ new byte[] { 137, 80, 78, 71 }, "png" }, // PNG 
				{ new byte[] { 73, 73, 42 }, "tiff" }, // TIFF 
				{ new byte[] { 77, 77, 42 }, "tiff" }, // TIFF 
				{ new byte[] { 255, 216, 255, 224 }, "jpg" }, // jpeg 
				{ new byte[] { 255, 216, 255, 225 }, "jpg" } // jpeg canon
			};

			var buffer = new byte[4];
			stream.Read(buffer, 0, buffer.Length);

			foreach (var testtype in types)
			{
				if (testtype.Key.SequenceEqual(buffer.Take(testtype.Key.Length)))
				{
					return testtype.Value;
				}
			}

			return "thumbnail"; //basically an error, or unknown filetype
		}

	}
}

public class ResponseFile
{
	public string FileName { get; set; }
	public Stream FileByteStream { get; set; }
}


public enum FILETYPE
{
	File_ID,
	File_Hash
}

/// <summary>
/// Verification information
/// </summary>
public class VERIFICATION
{
	public int[] Basic_permissions { get; set; }
	public string Human_description { get; set; }
}


/// <summary>
/// Api version
/// </summary>
public class API_VERSION
{
	public int Version { get; set; }
}

/// <summary>
/// Access Key
/// </summary>
public class ACCESS_KEY
{
	public string Access_Key { get; set; }
}

/// <summary>
/// Session Key
/// </summary>
public class SESSION_KEY
{
	public string Session_Key { get; set; }
}


