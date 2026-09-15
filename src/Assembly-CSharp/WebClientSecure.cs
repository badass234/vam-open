using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using MVR.FileManagement;

public class WebClientSecure : Component, IDisposable
{
	private WebClientWithTimeout client;

	private bool disposed;

	public int Timeout
	{
		get
		{
			return client.Timeout;
		}
		set
		{
			client.Timeout = value;
		}
	}

	public string BaseAddress
	{
		get
		{
			return client.BaseAddress;
		}
		set
		{
			if (ValidateAddress(value))
			{
				client.BaseAddress = value;
			}
		}
	}

	public ICredentials Credentials
	{
		get
		{
			return client.Credentials;
		}
		set
		{
			client.Credentials = value;
		}
	}

	public WebHeaderCollection Headers
	{
		get
		{
			return client.Headers;
		}
		set
		{
			client.Headers = value;
		}
	}

	public WebHeaderCollection ResponseHeaders => client.ResponseHeaders;

	public Encoding Encoding
	{
		get
		{
			return client.Encoding;
		}
		set
		{
			client.Encoding = value;
		}
	}

	public NameValueCollection QueryString
	{
		get
		{
			return client.QueryString;
		}
		set
		{
			client.QueryString = value;
		}
	}

	public IWebProxy Proxy
	{
		get
		{
			return client.Proxy;
		}
		set
		{
			client.Proxy = value;
		}
	}

	public bool UseDefaultCredentials
	{
		get
		{
			return client.UseDefaultCredentials;
		}
		set
		{
			client.UseDefaultCredentials = value;
		}
	}

	public event DownloadProgressChangedEventHandler DownloadProgressChanged
	{
		add
		{
			client.DownloadProgressChanged += value;
		}
		remove
		{
			client.DownloadProgressChanged -= value;
		}
	}

	public event UploadProgressChangedEventHandler UploadProgressChanged
	{
		add
		{
			client.UploadProgressChanged += value;
		}
		remove
		{
			client.UploadProgressChanged -= value;
		}
	}

	public event DownloadStringCompletedEventHandler DownloadStringCompleted
	{
		add
		{
			client.DownloadStringCompleted += value;
		}
		remove
		{
			client.DownloadStringCompleted -= value;
		}
	}

	public event DownloadDataCompletedEventHandler DownloadDataCompleted
	{
		add
		{
			client.DownloadDataCompleted += value;
		}
		remove
		{
			client.DownloadDataCompleted -= value;
		}
	}

	public event OpenReadCompletedEventHandler OpenReadCompleted
	{
		add
		{
			client.OpenReadCompleted += value;
		}
		remove
		{
			client.OpenReadCompleted -= value;
		}
	}

	public event OpenWriteCompletedEventHandler OpenWriteCompleted
	{
		add
		{
			client.OpenWriteCompleted += value;
		}
		remove
		{
			client.OpenWriteCompleted -= value;
		}
	}

	public event UploadStringCompletedEventHandler UploadStringCompleted
	{
		add
		{
			client.UploadStringCompleted += value;
		}
		remove
		{
			client.UploadStringCompleted -= value;
		}
	}

	public event UploadDataCompletedEventHandler UploadDataCompleted
	{
		add
		{
			client.UploadDataCompleted += value;
		}
		remove
		{
			client.UploadDataCompleted -= value;
		}
	}

	public event UploadFileCompletedEventHandler UploadFileCompleted
	{
		add
		{
			client.UploadFileCompleted += value;
		}
		remove
		{
			client.UploadFileCompleted -= value;
		}
	}

	public event UploadValuesCompletedEventHandler UploadValuesCompleted
	{
		add
		{
			client.UploadValuesCompleted += value;
		}
		remove
		{
			client.UploadValuesCompleted -= value;
		}
	}

	public WebClientSecure()
	{
		client = new WebClientWithTimeout();
	}

	private bool ValidateAddress(string address)
	{
		Uri result;
		if (Uri.TryCreate(address, UriKind.Absolute, out result) && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps))
		{
			return true;
		}
		string pluginHashDisplayName = FileManager.GetPluginHashDisplayName();
		if (pluginHashDisplayName != null)
		{
			SuperController.LogError("WebClientSecure only supports http or https addresses. " + pluginHashDisplayName + " tried to open " + address);
		}
		else
		{
			SuperController.LogError("WebClientSecure only supports http or https addresses. Code tried to open " + address);
		}
		return false;
	}

	private bool ValidateAddress(Uri address)
	{
		if (address.Scheme == Uri.UriSchemeHttp || address.Scheme == Uri.UriSchemeHttps)
		{
			return true;
		}
		string pluginHashDisplayName = FileManager.GetPluginHashDisplayName();
		if (pluginHashDisplayName != null)
		{
			SuperController.LogError("WebClientSecure only support http or https addresses. " + pluginHashDisplayName + " tried to open " + address);
		}
		else
		{
			SuperController.LogError("WebClientSecure only support http or https addresses. Code tried to open " + address);
		}
		return false;
	}

	public void CancelAsync()
	{
		client.CancelAsync();
	}

	public byte[] DownloadData(string address)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.DownloadData(address);
	}

	public byte[] DownloadData(Uri address)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.DownloadData(address);
	}

	public void DownloadDataAsync(Uri address)
	{
		if (ValidateAddress(address))
		{
			client.DownloadDataAsync(address);
		}
	}

	public void DownloadDataAsync(Uri address, object userToken)
	{
		if (ValidateAddress(address))
		{
			client.DownloadDataAsync(address, userToken);
		}
	}

	public string DownloadString(string address)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.DownloadString(address);
	}

	public string DownloadString(Uri address)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.DownloadString(address);
	}

	public void DownloadStringAsync(Uri address)
	{
		if (ValidateAddress(address))
		{
			client.DownloadStringAsync(address);
		}
	}

	public void DownloadStringAsync(Uri address, object userToken)
	{
		if (ValidateAddress(address))
		{
			client.DownloadStringAsync(address, userToken);
		}
	}

	public Stream OpenRead(string address)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.OpenRead(address);
	}

	public Stream OpenRead(Uri address)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.OpenRead(address);
	}

	public void OpenReadAsync(Uri address)
	{
		if (ValidateAddress(address))
		{
			client.OpenReadAsync(address);
		}
	}

	public void OpenReadAsync(Uri address, object userToken)
	{
		if (ValidateAddress(address))
		{
			client.OpenReadAsync(address, userToken);
		}
	}

	public Stream OpenWrite(string address)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.OpenWrite(address);
	}

	public Stream OpenWrite(Uri address)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.OpenWrite(address);
	}

	public Stream OpenWrite(string address, string method)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.OpenWrite(address, method);
	}

	public Stream OpenWrite(Uri address, string method)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.OpenWrite(address, method);
	}

	public void OpenWriteAsync(Uri address)
	{
		if (ValidateAddress(address))
		{
			client.OpenWriteAsync(address);
		}
	}

	public void OpenWriteAsync(Uri address, string method)
	{
		if (ValidateAddress(address))
		{
			client.OpenWriteAsync(address, method);
		}
	}

	public void OpenWriteAsync(Uri address, string method, object userToken)
	{
		if (ValidateAddress(address))
		{
			client.OpenWriteAsync(address, method, userToken);
		}
	}

	public byte[] UploadData(string address, byte[] data)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.UploadData(address, data);
	}

	public byte[] UploadData(Uri address, byte[] data)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.UploadData(address, data);
	}

	public byte[] UploadData(string address, string method, byte[] data)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.UploadData(address, method, data);
	}

	public byte[] UploadData(Uri address, string method, byte[] data)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.UploadData(address, method, data);
	}

	public void UploadDataAsync(Uri address, byte[] data)
	{
		if (ValidateAddress(address))
		{
			client.UploadDataAsync(address, data);
		}
	}

	public void UploadDataAsync(Uri address, string method, byte[] data)
	{
		if (ValidateAddress(address))
		{
			client.UploadDataAsync(address, method, data);
		}
	}

	public void UploadDataAsync(Uri address, string method, byte[] data, object userToken)
	{
		if (ValidateAddress(address))
		{
			client.UploadDataAsync(address, method, data, userToken);
		}
	}

	public string UploadString(string address, string data)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.UploadString(address, data);
	}

	public string UploadString(Uri address, string data)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.UploadString(address, data);
	}

	public string UploadString(string address, string method, string data)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.UploadString(address, method, data);
	}

	public string UploadString(Uri address, string method, string data)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.UploadString(address, method, data);
	}

	public void UploadStringAsync(Uri address, string data)
	{
		if (ValidateAddress(address))
		{
			client.UploadStringAsync(address, data);
		}
	}

	public void UploadStringAsync(Uri address, string method, string data)
	{
		if (ValidateAddress(address))
		{
			client.UploadStringAsync(address, method, data);
		}
	}

	public void UploadStringAsync(Uri address, string method, string data, object userToken)
	{
		if (ValidateAddress(address))
		{
			client.UploadStringAsync(address, method, data, userToken);
		}
	}

	public byte[] UploadValues(string address, NameValueCollection data)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.UploadValues(address, data);
	}

	public byte[] UploadValues(Uri address, NameValueCollection data)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.UploadValues(address, data);
	}

	public byte[] UploadValues(string address, string method, NameValueCollection data)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.UploadValues(address, method, data);
	}

	public byte[] UploadValues(Uri address, string method, NameValueCollection data)
	{
		if (!ValidateAddress(address))
		{
			return null;
		}
		return client.UploadValues(address, method, data);
	}

	public void UploadValuesAsync(Uri address, NameValueCollection data)
	{
		if (ValidateAddress(address))
		{
			client.UploadValuesAsync(address, data);
		}
	}

	public void UploadValuesAsync(Uri address, string method, NameValueCollection data)
	{
		if (ValidateAddress(address))
		{
			client.UploadValuesAsync(address, method, data);
		}
	}

	public void UploadValuesAsync(Uri address, string method, NameValueCollection data, object userToken)
	{
		if (ValidateAddress(address))
		{
			client.UploadValuesAsync(address, method, data, userToken);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!disposed)
		{
			if (disposing && client != null)
			{
				client.Dispose();
				client = null;
			}
			disposed = true;
		}
		base.Dispose(disposing);
	}

	public new void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}
}
