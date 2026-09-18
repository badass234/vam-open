using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace IKVM.Reflection
{
	public sealed class StrongNameKeyPair
	{
		private readonly byte[] keyPairArray;

		private readonly string keyPairContainer;

		public byte[] PublicKey
		{
			get
			{
				if (Universe.MonoRuntime)
				{
					return MonoGetPublicKey();
				}
				using (RSACryptoServiceProvider rSACryptoServiceProvider = CreateRSA())
				{
					byte[] array = rSACryptoServiceProvider.ExportCspBlob(false);
					byte[] array2 = new byte[12 + array.Length];
					Buffer.BlockCopy(array, 0, array2, 12, array.Length);
					array2[1] = 36;
					array2[4] = 4;
					array2[5] = 128;
					array2[8] = (byte)array.Length;
					array2[9] = (byte)(array.Length >> 8);
					array2[10] = (byte)(array.Length >> 16);
					array2[11] = (byte)(array.Length >> 24);
					return array2;
				}
			}
		}

		public StrongNameKeyPair(string keyPairContainer)
		{
			if (keyPairContainer == null)
			{
				throw new ArgumentNullException("keyPairContainer");
			}
			if (Universe.MonoRuntime && Environment.OSVersion.Platform == PlatformID.Win32NT)
			{
				throw new NotSupportedException("IKVM.Reflection does not support key containers when running on Mono");
			}
			this.keyPairContainer = keyPairContainer;
		}

		public StrongNameKeyPair(byte[] keyPairArray)
		{
			if (keyPairArray == null)
			{
				throw new ArgumentNullException("keyPairArray");
			}
			this.keyPairArray = (byte[])keyPairArray.Clone();
		}

		public StrongNameKeyPair(FileStream keyPairFile)
			: this(ReadAllBytes(keyPairFile))
		{
		}

		private static byte[] ReadAllBytes(FileStream keyPairFile)
		{
			if (keyPairFile == null)
			{
				throw new ArgumentNullException("keyPairFile");
			}
			byte[] array = new byte[keyPairFile.Length - keyPairFile.Position];
			keyPairFile.Read(array, 0, array.Length);
			return array;
		}

		internal RSACryptoServiceProvider CreateRSA()
		{
			try
			{
				if (keyPairArray != null)
				{
					RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider();
					rSACryptoServiceProvider.ImportCspBlob(keyPairArray);
					return rSACryptoServiceProvider;
				}
				CspParameters cspParameters = new CspParameters();
				cspParameters.KeyContainerName = keyPairContainer;
				if (!Universe.MonoRuntime)
				{
					cspParameters.Flags = CspProviderFlags.UseMachineKeyStore | CspProviderFlags.UseExistingKey;
					cspParameters.KeyNumber = 2;
				}
				return new RSACryptoServiceProvider(cspParameters);
			}
			catch
			{
				throw new ArgumentException("Unable to obtain public key for StrongNameKeyPair.");
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private byte[] MonoGetPublicKey()
		{
			if (keyPairArray == null)
			{
				return new System.Reflection.StrongNameKeyPair(keyPairContainer).PublicKey;
			}
			return new System.Reflection.StrongNameKeyPair(keyPairArray).PublicKey;
		}
	}
}
