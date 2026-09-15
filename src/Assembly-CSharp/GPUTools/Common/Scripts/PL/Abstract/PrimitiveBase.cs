using System;
using System.Collections.Generic;
using System.Reflection;
using GPUTools.Common.Scripts.PL.Attributes;
using UnityEngine;

namespace GPUTools.Common.Scripts.PL.Abstract
{
	public class PrimitiveBase : IPass
	{
		private readonly List<IPass> passes = new List<IPass>();

		private readonly List<List<KeyValuePair<GpuData, PropertyInfo>>> passesAttributes = new List<List<KeyValuePair<GpuData, PropertyInfo>>>();

		private readonly List<KeyValuePair<GpuData, PropertyInfo>> ownAttributes = new List<KeyValuePair<GpuData, PropertyInfo>>();

		protected void Bind()
		{
			CachePassAttributes();
			CacheOwnAttributes();
			BindAttributes();
		}

		public virtual void Dispatch()
		{
			for (int i = 0; i < passes.Count; i++)
			{
				passes[i].Dispatch();
			}
		}

		public virtual void Dispose()
		{
			for (int i = 0; i < passes.Count; i++)
			{
				passes[i].Dispose();
			}
		}

		public void AddPass(IPass pass)
		{
			passes.Add(pass);
		}

		public void RemovePass(IPass pass)
		{
			if (!passes.Contains(pass))
			{
				Debug.LogError("Can't find pass");
			}
			else
			{
				passes.Remove(pass);
			}
		}

		private void CachePassAttributes()
		{
			passesAttributes.Clear();
			for (int i = 0; i < passes.Count; i++)
			{
				IPass pass = passes[i];
				PropertyInfo[] properties = pass.GetType().GetProperties();
				List<KeyValuePair<GpuData, PropertyInfo>> list = new List<KeyValuePair<GpuData, PropertyInfo>>();
				passesAttributes.Add(list);
				foreach (PropertyInfo propertyInfo in properties)
				{
					if (Attribute.IsDefined(propertyInfo, typeof(GpuData)))
					{
						GpuData key = (GpuData)Attribute.GetCustomAttribute(propertyInfo, typeof(GpuData));
						list.Add(new KeyValuePair<GpuData, PropertyInfo>(key, propertyInfo));
					}
				}
			}
		}

		private void CacheOwnAttributes()
		{
			ownAttributes.Clear();
			PropertyInfo[] properties = GetType().GetProperties();
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (Attribute.IsDefined(propertyInfo, typeof(GpuData)))
				{
					GpuData key = (GpuData)Attribute.GetCustomAttribute(propertyInfo, typeof(GpuData));
					ownAttributes.Add(new KeyValuePair<GpuData, PropertyInfo>(key, propertyInfo));
				}
			}
		}

		protected void BindAttributes()
		{
			for (int i = 0; i < ownAttributes.Count; i++)
			{
				KeyValuePair<GpuData, PropertyInfo> keyValuePair = ownAttributes[i];
				for (int j = 0; j < passesAttributes.Count; j++)
				{
					List<KeyValuePair<GpuData, PropertyInfo>> list = passesAttributes[j];
					for (int k = 0; k < list.Count; k++)
					{
						KeyValuePair<GpuData, PropertyInfo> keyValuePair2 = list[k];
						if (keyValuePair2.Key.Name.Equals(keyValuePair.Key.Name))
						{
							keyValuePair2.Value.SetValue(passes[j], keyValuePair.Value.GetValue(this, null), null);
						}
					}
				}
			}
		}
	}
}
