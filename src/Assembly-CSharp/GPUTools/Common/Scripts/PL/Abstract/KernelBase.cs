using System;
using System.Collections.Generic;
using System.Reflection;
using GPUTools.Common.Scripts.PL.Attributes;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Common.Scripts.Utils;
using UnityEngine;

namespace GPUTools.Common.Scripts.PL.Abstract
{
	public class KernelBase : IPass
	{
		protected readonly string KernalName;

		protected readonly int KernelId;

		protected readonly List<KeyValuePair<GpuData, object>> Props = new List<KeyValuePair<GpuData, object>>();

		protected readonly Dictionary<IBufferWrapper, string> BufferToLengthAttributeName = new Dictionary<IBufferWrapper, string>();

		public bool IsEnabled { get; set; }

		protected ComputeShader Shader { get; private set; }

		public KernelBase(string shaderPath, string kernelName)
		{
			Shader = Resources.Load<ComputeShader>(shaderPath);
			KernalName = kernelName;
			KernelId = Shader.FindKernel(kernelName);
			IsEnabled = true;
		}

		public virtual void Dispatch()
		{
			if (IsEnabled)
			{
				if (Props.Count == 0)
				{
					CacheAttributes();
				}
				BindAttributes();
				int groupsNumX = GetGroupsNumX();
				int groupsNumY = GetGroupsNumY();
				int groupsNumZ = GetGroupsNumZ();
				if (groupsNumX != 0 && groupsNumY != 0 && groupsNumZ != 0)
				{
					Shader.Dispatch(KernelId, groupsNumX, groupsNumY, groupsNumZ);
				}
			}
		}

		public virtual void Dispose()
		{
		}

		public virtual int GetGroupsNumX()
		{
			return 1;
		}

		public virtual int GetGroupsNumY()
		{
			return 1;
		}

		public virtual int GetGroupsNumZ()
		{
			return 1;
		}

		public void ClearCacheAttributes()
		{
			CacheAttributes();
		}

		protected virtual void CacheAttributes()
		{
			Props.Clear();
			BufferToLengthAttributeName.Clear();
			PropertyInfo[] properties = GetType().GetProperties();
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				if (Attribute.IsDefined(propertyInfo, typeof(GpuData)))
				{
					GpuData gpuData = (GpuData)Attribute.GetCustomAttribute(propertyInfo, typeof(GpuData));
					object value = propertyInfo.GetValue(this, null);
					if (value is IBufferWrapper)
					{
						BufferToLengthAttributeName.Add(value as IBufferWrapper, gpuData.Name + "Length");
					}
					Props.Add(new KeyValuePair<GpuData, object>(gpuData, value));
				}
			}
		}

		protected void BindAttributes()
		{
			for (int i = 0; i < Props.Count; i++)
			{
				GpuData key = Props[i].Key;
				object value = Props[i].Value;
				if (value is IBufferWrapper)
				{
					IBufferWrapper bufferWrapper = (IBufferWrapper)value;
					ComputeBuffer computeBuffer = bufferWrapper.ComputeBuffer;
					if (computeBuffer != null)
					{
						if (!computeBuffer.IsValid())
						{
							Debug.LogError("Compute buffer " + computeBuffer.GetHashCode() + " is not valid for " + KernalName + " " + key.Name);
						}
						else
						{
							Shader.SetBuffer(KernelId, key.Name, computeBuffer);
							string value2;
							if (BufferToLengthAttributeName.TryGetValue(bufferWrapper, out value2))
							{
								Shader.SetInt(value2, computeBuffer.count);
							}
						}
					}
					else
					{
						Debug.LogError("Null compute buffer for " + KernalName);
					}
				}
				else if (value is Texture)
				{
					Shader.SetTexture(KernelId, key.Name, (Texture)value);
				}
				else if (value is GpuValue<int>)
				{
					Shader.SetInt(key.Name, ((GpuValue<int>)value).Value);
				}
				else if (value is GpuValue<float>)
				{
					Shader.SetFloat(key.Name, ((GpuValue<float>)value).Value);
				}
				else if (value is GpuValue<Vector3>)
				{
					Shader.SetVector(key.Name, ((GpuValue<Vector3>)value).Value);
				}
				else if (value is GpuValue<Color>)
				{
					Shader.SetVector(key.Name, ((GpuValue<Color>)value).Value.ToVector());
				}
				else if (value is GpuValue<bool>)
				{
					Shader.SetBool(key.Name, ((GpuValue<bool>)value).Value);
				}
				else if (value is GpuValue<GpuMatrix4x4>)
				{
					Shader.SetFloats(key.Name, ((GpuValue<GpuMatrix4x4>)value).Value.Values);
				}
			}
		}
	}
}
