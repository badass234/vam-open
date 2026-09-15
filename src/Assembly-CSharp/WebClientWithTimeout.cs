using System;
using System.Net;

public class WebClientWithTimeout : WebClient
{
	public int Timeout { get; set; }

	public WebClientWithTimeout()
	{
		Timeout = 100000;
	}

	protected override WebRequest GetWebRequest(Uri address)
	{
		WebRequest webRequest = base.GetWebRequest(address);
		webRequest.Timeout = Timeout;
		return webRequest;
	}
}
