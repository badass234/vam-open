using System;

namespace Oculus.Platform.Models
{
	public class SystemPermission
	{
		public readonly bool HasPermission;

		public readonly PermissionGrantStatus PermissionGrantStatus;

		public SystemPermission(IntPtr o)
		{
			HasPermission = CAPI.ovr_SystemPermission_GetHasPermission(o);
			PermissionGrantStatus = CAPI.ovr_SystemPermission_GetPermissionGrantStatus(o);
		}
	}
}
