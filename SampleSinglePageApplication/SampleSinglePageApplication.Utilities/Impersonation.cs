using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace SampleSinglePageApplication;

//https://docs.microsoft.com/en-us/dotnet/api/system.security.principal.windowsidentity.runimpersonated?view=windowsdesktop-5.0
public class UserImpersonation : IDisposable
{
	[DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
	public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
			int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);

	// This class wraps around a section of code and runs that code as an impersonated user
	private readonly SafeAccessTokenHandle _handle;
	private string _domain;
	private string _username;
	private string _password;
	private bool _authenticated;

	const int LOGON32_PROVIDER_DEFAULT = 0;
	const int LOGON32_LOGON_INTERACTIVE = 2;

	public UserImpersonation(string domain, string username, string password)
	{
		_domain = domain;
		_username = username;
		_password = password;

		if (String.IsNullOrWhiteSpace(_domain) || String.IsNullOrWhiteSpace(_username) || String.IsNullOrWhiteSpace(_password)) {
			throw new NullReferenceException("Domain, Username, and Password are required.");
		}

		_authenticated = LogonUser(_username, _domain, _password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out this._handle);
		if (!_authenticated) {
			var errorCode = Marshal.GetLastWin32Error();
			throw new ApplicationException(string.Format("Could not impersonate the elevated user.  LogonUser returned error code {0}.", errorCode));
		}
	}

	public T RunImpersonated<T>(Func<T> function)
	{
		T output = default(T);

#pragma warning disable CA1416 // Validate platform compatibility
		WindowsIdentity.RunImpersonated(this._handle, () => {
			output = function();
		});
#pragma warning restore CA1416 // Validate platform compatibility

		return output;
	}

	public void Dispose()
	{
		this._handle.Dispose();
	}
}