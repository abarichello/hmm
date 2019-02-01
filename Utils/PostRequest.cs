using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Pocketverse;

namespace HeavyMetalMachines.Utils
{
	public static class PostRequest
	{
		public static void Post(string url, string email)
		{
			PostRequest.PostCall @object = new PostRequest.PostCall
			{
				Form = "<html>\r\n<form action=\"{0}\" method=\"post\">\r\n    <input type=\"hidden\" name=\"email\" value=\"{1}\"/>\r\n\t<script type=\"text/javascript\">\r\n\t\twindow.onload = function()\r\n\t\t{{\r\n            if (\"WebSocket\" in window)\r\n            {{\r\n                var ws = new WebSocket(\"ws://localhost:{2}/\");\r\n                ws.onclose = function()\r\n                    {{\r\n                        document.forms[0].submit();\r\n                    }}\r\n            }}\r\n            else\r\n            {{\r\n                document.forms[0].submit();\r\n            }}\r\n            return;\r\n        }}\r\n\t</script>\r\n</form>\r\n</html>\r\n",
				Url = url,
				Email = email
			};
			Thread thread = new Thread(new ThreadStart(@object.Call))
			{
				Name = "Post call thread"
			};
			thread.Start();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PostRequest));

		private const int WebSocketTimeout = 120;

		private const string DefaultPostForm = "<html>\r\n<form action=\"{0}\" method=\"post\">\r\n    <input type=\"hidden\" name=\"email\" value=\"{1}\"/>\r\n\t<script type=\"text/javascript\">\r\n\t\twindow.onload = function()\r\n\t\t{{\r\n            if (\"WebSocket\" in window)\r\n            {{\r\n                var ws = new WebSocket(\"ws://localhost:{2}/\");\r\n                ws.onclose = function()\r\n                    {{\r\n                        document.forms[0].submit();\r\n                    }}\r\n            }}\r\n            else\r\n            {{\r\n                document.forms[0].submit();\r\n            }}\r\n            return;\r\n        }}\r\n\t</script>\r\n</form>\r\n</html>\r\n";

		private class PostCall
		{
			public void Call()
			{
				try
				{
					TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
					tcpListener.Server.ReceiveTimeout = 500;
					tcpListener.Server.SendTimeout = 500;
					tcpListener.Start();
					this._port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
					string tempFileName = Path.GetTempFileName();
					using (FileStream fileStream = new FileStream(tempFileName, FileMode.OpenOrCreate))
					{
						string s = string.Format(this.Form, this.Url, this.Email, this._port);
						byte[] bytes = Encoding.UTF8.GetBytes(s);
						fileStream.Write(bytes, 0, bytes.Length);
					}
					FileInfo fileInfo = new FileInfo(tempFileName);
					fileInfo.MoveTo(tempFileName + ".html");
					Process.Start(fileInfo.FullName);
					DateTime t = DateTime.Now + TimeSpan.FromSeconds(120.0);
					while (!tcpListener.Pending() && DateTime.Now < t)
					{
						Thread.Sleep(10);
					}
					if (tcpListener.Pending())
					{
						tcpListener.AcceptTcpClient();
					}
					tcpListener.Stop();
					fileInfo.Delete();
				}
				catch (Exception e)
				{
					PostRequest.Log.Fatal(string.Format("Failed to open link={0} for user={1}", this.Url, this.Email), e);
				}
			}

			public string Form;

			public string Url;

			public string Email;

			private int _port;
		}
	}
}
