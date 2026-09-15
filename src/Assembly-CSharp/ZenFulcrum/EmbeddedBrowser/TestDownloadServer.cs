using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser
{
	public class TestDownloadServer : MonoBehaviour
	{
		private HttpListener server;

		public int port = 8083;

		private volatile bool serverEnabled = true;

		public void OnEnable()
		{
			server = new HttpListener();
			server.Prefixes.Add("http://localhost:" + port + "/");
			server.Start();
			serverEnabled = true;
			new Thread(ListenThread).Start();
		}

		private void ListenThread()
		{
			while (serverEnabled)
			{
				HttpListenerContext context = server.GetContext();
				new Thread(ResponseThread).Start(context);
			}
		}

		private void ResponseThread(object obj)
		{
			HttpListenerContext httpListenerContext = (HttpListenerContext)obj;
			HttpListenerResponse res = httpListenerContext.Response;
			res.StatusCode = 200;
			StreamWriter output = new StreamWriter(res.OutputStream);
			Action action = delegate
			{
				string text = "Lorem ipsum dolor sit amet.\n";
				int num = 1024;
				res.AddHeader("Content-length", (text.Length * num).ToString());
				res.AddHeader("Content-type", "application/octet-stream");
				for (int i = 0; i < num; i++)
				{
					output.Write(text);
					Thread.Sleep(1);
				}
			};
			string absolutePath = httpListenerContext.Request.Url.AbsolutePath;
			switch (absolutePath)
			{
			case "/basicFile":
				action();
				break;
			case "/bigFile":
			{
				string text2 = "Lorem ipsum dolor sit amet.\n";
				long num2 = 104857600L;
				res.AddHeader("Content-length", (text2.Length * num2).ToString());
				res.AddHeader("Content-type", "application/octet-stream");
				byte[] bytes = Encoding.ASCII.GetBytes(text2);
				byte[] array = new byte[1024 * bytes.Length];
				for (int l = 0; l < 1024; l++)
				{
					Array.Copy(bytes, 0, array, l * bytes.Length, bytes.Length);
				}
				for (int m = 0; m < num2 / 1024; m++)
				{
					res.OutputStream.Write(array, 0, array.Length);
				}
				break;
			}
			case "/slowFile":
			case "/slowPage":
			{
				string text3 = "Lorem ipsum dolor sit amet.\n";
				int num3 = 1048576;
				res.AddHeader("Content-length", (text3.Length * num3).ToString());
				res.AddHeader("Content-type", (!(absolutePath == "/slowFile")) ? "text/plain" : "application/octet-stream");
				for (int n = 0; n < num3; n++)
				{
					output.Write(text3);
					Thread.Sleep(1);
				}
				break;
			}
			case "/textFile":
			{
				res.AddHeader("Content-type", "text/plain");
				for (int k = 0; k < 100; k++)
				{
					output.Write("This is some text!\n");
				}
				break;
			}
			case "/textFileDownload":
			{
				res.AddHeader("Content-type", "text/plain");
				res.AddHeader("Content-Disposition", "attachment; filename=\"A Great Document Full of Text.txt\"");
				for (int j = 0; j < 100; j++)
				{
					output.Write("This is some text!\n");
				}
				break;
			}
			case "/ǝpoɔıun«ñämé»":
			case "/%C7%9Dpo%C9%94%C4%B1un%C2%AB%C3%B1%C3%A4m%C3%A9%C2%BB":
				action();
				break;
			case "/redirectedFile":
				res.StatusCode = 302;
				res.AddHeader("Location", "/some/other/file/i/was/redirected/to/redirectedResult");
				break;
			case "/some/other/file/i/was/redirected/to/redirectedResult":
				action();
				break;
			default:
				httpListenerContext.Response.StatusCode = 404;
				output.Write("Not found");
				break;
			}
			output.Close();
		}

		public void OnDisable()
		{
			serverEnabled = false;
			server.Stop();
		}
	}
}
