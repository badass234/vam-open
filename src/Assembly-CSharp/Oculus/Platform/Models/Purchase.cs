using System;

namespace Oculus.Platform.Models
{
	public class Purchase
	{
		public readonly DateTime ExpirationTime;

		public readonly DateTime GrantTime;

		public readonly ulong ID;

		public readonly string Sku;

		public Purchase(IntPtr o)
		{
			ExpirationTime = CAPI.ovr_Purchase_GetExpirationTime(o);
			GrantTime = CAPI.ovr_Purchase_GetGrantTime(o);
			ID = CAPI.ovr_Purchase_GetPurchaseID(o);
			Sku = CAPI.ovr_Purchase_GetSKU(o);
		}
	}
}
