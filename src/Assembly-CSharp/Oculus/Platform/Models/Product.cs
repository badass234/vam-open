using System;

namespace Oculus.Platform.Models
{
	public class Product
	{
		public readonly string Description;

		public readonly string FormattedPrice;

		public readonly string Name;

		public readonly string Sku;

		public Product(IntPtr o)
		{
			Description = CAPI.ovr_Product_GetDescription(o);
			FormattedPrice = CAPI.ovr_Product_GetFormattedPrice(o);
			Name = CAPI.ovr_Product_GetName(o);
			Sku = CAPI.ovr_Product_GetSKU(o);
		}
	}
}
