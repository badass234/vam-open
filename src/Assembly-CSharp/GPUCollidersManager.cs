using System.Collections.Generic;
using GPUTools.Common.Scripts.PL.Tools;
using GPUTools.Physics.Scripts.Behaviours;
using GPUTools.Physics.Scripts.Kernels;
using GPUTools.Physics.Scripts.Types.Shapes;
using UnityEngine;

public class GPUCollidersManager : MonoBehaviour
{
	public static GPUCollidersManager singleton;

	protected GpuBuffer<GPLineSphere> lineSpheres;

	protected GpuBuffer<GPLineSphere> oldLineSpheres;

	protected GpuBuffer<GPLineSphereWithDelta> processedLineSpheres;

	protected GpuBuffer<GPSphere> spheres;

	protected GpuBuffer<GPSphere> oldSpheres;

	protected GpuBuffer<GPSphereWithDelta> processedSpheres;

	protected GpuBuffer<Vector4> planes;

	protected List<GPUCollidersConsumer> consumers;

	protected List<GpuSphereCollider> sphereColliders;

	protected List<CapsuleLineSphereCollider> lineSphereColliders;

	protected List<PlaneCollider> planeColliders;

	private ParticleLineSphereCopyKernel lineSphereCopyKernel;

	private ParticleSphereCopyKernel sphereCopyKernel;

	private ParticleLineSphereProcessKernel lineSphereProcessKernel;

	private ParticleSphereProcessKernel sphereProcessKernel;

	protected bool usingNullGrabSpheresBuff;

	protected GpuBuffer<GPGrabSphere> grabSpheresBuff;

	protected List<GpuGrabSphere> grabSpheres;

	protected Dictionary<int, bool> grabSphereIDs;

	protected int uidcount;

	protected GpuBuffer<GPLineSphere> cutCapsulesBuff;

	protected List<GpuEditCapsule> cutCapsules;

	protected GpuBuffer<GPLineSphere> growCapsulesBuff;

	protected List<GpuEditCapsule> growCapsules;

	protected GpuBuffer<GPLineSphere> holdCapsulesBuff;

	protected List<GpuEditCapsule> holdCapsules;

	protected GpuBuffer<GPLineSphereWithMatrixDelta> grabCapsulesBuff;

	protected List<GpuEditCapsule> grabCapsules;

	protected GpuBuffer<GPLineSphere> pushCapsulesBuff;

	protected List<GpuEditCapsule> pushCapsules;

	protected GpuBuffer<GPLineSphere> pullCapsulesBuff;

	protected List<GpuEditCapsule> pullCapsules;

	protected GpuBuffer<GPLineSphereWithDelta> brushCapsulesBuff;

	protected List<GpuEditCapsule> brushCapsules;

	protected GpuBuffer<GPLineSphere> rigidityIncreaseCapsulesBuff;

	protected List<GpuEditCapsule> rigidityIncreaseCapsules;

	protected GpuBuffer<GPLineSphere> rigidityDecreaseCapsulesBuff;

	protected List<GpuEditCapsule> rigidityDecreaseCapsules;

	protected GpuBuffer<GPLineSphere> rigiditySetCapsulesBuff;

	protected List<GpuEditCapsule> rigiditySetCapsules;

	protected bool _wasInit;

	public float fixedFrame1Prediction = 1.2f;

	public float fixedFrame2Prediction = 1.2f;

	public float fixedFrame3Prediction = 1.2f;

	protected float _collisionPrediction;

	protected int fixedDispatchCount;

	public static GpuBuffer<GPLineSphereWithDelta> processedLineSpheresBuffer
	{
		get
		{
			if (singleton == null)
			{
				return null;
			}
			return singleton.processedLineSpheres;
		}
	}

	public static GpuBuffer<GPSphereWithDelta> processedSpheresBuffer
	{
		get
		{
			if (singleton == null)
			{
				return null;
			}
			return singleton.processedSpheres;
		}
	}

	public static GpuBuffer<Vector4> planesBuffer
	{
		get
		{
			if (singleton == null)
			{
				return null;
			}
			return singleton.planes;
		}
	}

	public static GpuBuffer<GPGrabSphere> grabSpheresBuffer
	{
		get
		{
			if (singleton == null)
			{
				return null;
			}
			return singleton.grabSpheresBuff;
		}
	}

	public static GpuBuffer<GPLineSphere> cutLineSpheresBuffer
	{
		get
		{
			if (singleton == null)
			{
				return null;
			}
			return singleton.cutCapsulesBuff;
		}
	}

	public static GpuBuffer<GPLineSphere> growLineSpheresBuffer
	{
		get
		{
			if (singleton == null)
			{
				return null;
			}
			return singleton.growCapsulesBuff;
		}
	}

	public static GpuBuffer<GPLineSphere> holdLineSpheresBuffer
	{
		get
		{
			if (singleton == null)
			{
				return null;
			}
			return singleton.holdCapsulesBuff;
		}
	}

	public static GpuBuffer<GPLineSphereWithMatrixDelta> grabLineSpheresBuffer
	{
		get
		{
			if (singleton == null)
			{
				return null;
			}
			return singleton.grabCapsulesBuff;
		}
	}

	public static GpuBuffer<GPLineSphere> pushLineSpheresBuffer
	{
		get
		{
			if (singleton == null)
			{
				return null;
			}
			return singleton.pushCapsulesBuff;
		}
	}

	public static GpuBuffer<GPLineSphere> pullLineSpheresBuffer
	{
		get
		{
			if (singleton == null)
			{
				return null;
			}
			return singleton.pullCapsulesBuff;
		}
	}

	public static GpuBuffer<GPLineSphereWithDelta> brushLineSpheresBuffer
	{
		get
		{
			if (singleton == null)
			{
				return null;
			}
			return singleton.brushCapsulesBuff;
		}
	}

	public static GpuBuffer<GPLineSphere> rigidityIncreaseLineSpheresBuffer
	{
		get
		{
			if (singleton == null)
			{
				return null;
			}
			return singleton.rigidityIncreaseCapsulesBuff;
		}
	}

	public static GpuBuffer<GPLineSphere> rigidityDecreaseLineSpheresBuffer
	{
		get
		{
			if (singleton == null)
			{
				return null;
			}
			return singleton.rigidityDecreaseCapsulesBuff;
		}
	}

	public static GpuBuffer<GPLineSphere> rigiditySetLineSpheresBuffer
	{
		get
		{
			if (singleton == null)
			{
				return null;
			}
			return singleton.rigiditySetCapsulesBuff;
		}
	}

	public static void RegisterLineSphereCollider(CapsuleLineSphereCollider lsc)
	{
		if (singleton != null)
		{
			singleton.RegLineSphereCollider(lsc);
		}
	}

	public static void DeregisterLineSphereCollider(CapsuleLineSphereCollider lsc)
	{
		if (singleton != null)
		{
			singleton.DeregLineSphereCollider(lsc);
		}
	}

	public static void RegisterSphereCollider(GpuSphereCollider sc)
	{
		if (singleton != null)
		{
			singleton.RegSphereCollider(sc);
		}
	}

	public static void DeregisterSphereCollider(GpuSphereCollider sc)
	{
		if (singleton != null)
		{
			singleton.DeregSphereCollider(sc);
		}
	}

	public static void RegisterPlaneCollider(PlaneCollider pc)
	{
		if (singleton != null)
		{
			singleton.RegPlaneCollider(pc);
		}
	}

	public static void DeregisterPlaneCollider(PlaneCollider pc)
	{
		if (singleton != null)
		{
			singleton.DeregPlaneCollider(pc);
		}
	}

	public static void RegisterConsumer(GPUCollidersConsumer consumer)
	{
		if (singleton != null)
		{
			singleton.RegConsumer(consumer);
		}
	}

	public static void DeregisterConsumer(GPUCollidersConsumer consumer)
	{
		if (singleton != null)
		{
			singleton.DeregConsumer(consumer);
		}
	}

	public static void RegisterGrabSphere(GpuGrabSphere gs)
	{
		if (singleton != null)
		{
			singleton.RegGrabSphere(gs);
		}
	}

	public static void DeregisterGrabSphere(GpuGrabSphere gs)
	{
		if (singleton != null)
		{
			singleton.DeregGrabSphere(gs);
		}
	}

	public static void RegisterCutCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.RegCutCapsule(gs);
		}
	}

	public static void DeregisterCutCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.DeregCutCapsule(gs);
		}
	}

	public static void RegisterGrowCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.RegGrowCapsule(gs);
		}
	}

	public static void DeregisterGrowCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.DeregGrowCapsule(gs);
		}
	}

	public static void RegisterHoldCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.RegHoldCapsule(gs);
		}
	}

	public static void DeregisterHoldCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.DeregHoldCapsule(gs);
		}
	}

	public static void RegisterGrabCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.RegGrabCapsule(gs);
		}
	}

	public static void DeregisterGrabCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.DeregGrabCapsule(gs);
		}
	}

	public static void RegisterPushCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.RegPushCapsule(gs);
		}
	}

	public static void DeregisterPushCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.DeregPushCapsule(gs);
		}
	}

	public static void RegisterPullCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.RegPullCapsule(gs);
		}
	}

	public static void DeregisterPullCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.DeregPullCapsule(gs);
		}
	}

	public static void RegisterBrushCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.RegBrushCapsule(gs);
		}
	}

	public static void DeregisterBrushCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.DeregBrushCapsule(gs);
		}
	}

	public static void RegisterRigidityIncreaseCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.RegRigidityIncreaseCapsule(gs);
		}
	}

	public static void DeregisterRigidityIncreaseCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.DeregRigidityIncreaseCapsule(gs);
		}
	}

	public static void RegisterRigidityDecreaseCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.RegRigidityDecreaseCapsule(gs);
		}
	}

	public static void DeregisterRigidityDecreaseCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.DeregRigidityDecreaseCapsule(gs);
		}
	}

	public static void RegisterRigiditySetCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.RegRigiditySetCapsule(gs);
		}
	}

	public static void DeregisterRigiditySetCapsule(GpuEditCapsule gs)
	{
		if (singleton != null)
		{
			singleton.DeregRigiditySetCapsule(gs);
		}
	}

	protected void ComputeColliders()
	{
		if (consumers.Count <= 0)
		{
			return;
		}
		if (spheres == null || spheres.Count != sphereColliders.Count)
		{
			if (spheres != null)
			{
				spheres.Dispose();
			}
			if (processedSpheres != null)
			{
				processedSpheres.Dispose();
			}
			if (sphereColliders.Count > 0)
			{
				spheres = new GpuBuffer<GPSphere>(sphereColliders.Count, GPSphere.Size());
				processedSpheres = new GpuBuffer<GPSphereWithDelta>(sphereColliders.Count, GPSphereWithDelta.Size());
			}
			else
			{
				spheres = null;
				processedSpheres = null;
			}
			sphereCopyKernel.Spheres = spheres;
			sphereCopyKernel.ClearCacheAttributes();
			sphereProcessKernel.Spheres = spheres;
			sphereProcessKernel.ProcessedSpheres = processedSpheres;
			sphereProcessKernel.ClearCacheAttributes();
		}
		bool flag = false;
		if (oldSpheres == null || oldSpheres.Count != sphereColliders.Count)
		{
			flag = true;
			if (oldSpheres != null)
			{
				oldSpheres.Dispose();
			}
			if (sphereColliders.Count > 0)
			{
				oldSpheres = new GpuBuffer<GPSphere>(sphereColliders.Count, GPSphere.Size());
			}
			else
			{
				oldSpheres = null;
			}
			sphereCopyKernel.OldSpheres = oldSpheres;
			sphereCopyKernel.ClearCacheAttributes();
			sphereProcessKernel.OldSpheres = oldSpheres;
			sphereProcessKernel.ClearCacheAttributes();
		}
		if (spheres != null && oldSpheres != null)
		{
			GPSphere[] data = oldSpheres.Data;
			GPSphere[] data2 = spheres.Data;
			if (!flag)
			{
				sphereCopyKernel.Dispatch();
			}
			for (int i = 0; i < sphereColliders.Count; i++)
			{
				GpuSphereCollider gpuSphereCollider = sphereColliders[i];
				data2[i].Position = gpuSphereCollider.worldCenter;
				data2[i].Radius = gpuSphereCollider.worldRadius;
				data2[i].Friction = gpuSphereCollider.friction;
			}
			spheres.PushData();
			if (flag)
			{
				sphereCopyKernel.Dispatch();
			}
			sphereProcessKernel.Dispatch();
		}
		if (lineSpheres == null || lineSpheres.Count != lineSphereColliders.Count)
		{
			if (lineSpheres != null)
			{
				lineSpheres.Dispose();
			}
			if (processedLineSpheres != null)
			{
				processedLineSpheres.Dispose();
			}
			if (lineSphereColliders.Count > 0)
			{
				lineSpheres = new GpuBuffer<GPLineSphere>(lineSphereColliders.Count, GPLineSphere.Size());
				processedLineSpheres = new GpuBuffer<GPLineSphereWithDelta>(lineSphereColliders.Count, GPLineSphereWithDelta.Size());
			}
			else
			{
				lineSpheres = null;
				processedLineSpheres = null;
			}
			lineSphereCopyKernel.LineSpheres = lineSpheres;
			lineSphereCopyKernel.ClearCacheAttributes();
			lineSphereProcessKernel.LineSpheres = lineSpheres;
			lineSphereProcessKernel.ProcessedLineSpheres = processedLineSpheresBuffer;
			lineSphereProcessKernel.ClearCacheAttributes();
		}
		bool flag2 = false;
		if (oldLineSpheres == null || oldLineSpheres.Count != lineSphereColliders.Count)
		{
			flag2 = true;
			if (oldLineSpheres != null)
			{
				oldLineSpheres.Dispose();
			}
			if (lineSphereColliders.Count > 0)
			{
				oldLineSpheres = new GpuBuffer<GPLineSphere>(lineSphereColliders.Count, GPLineSphere.Size());
			}
			else
			{
				oldLineSpheres = null;
			}
			lineSphereCopyKernel.OldLineSpheres = oldLineSpheres;
			lineSphereCopyKernel.ClearCacheAttributes();
			lineSphereProcessKernel.OldLineSpheres = oldLineSpheres;
			lineSphereProcessKernel.ClearCacheAttributes();
		}
		if (lineSpheres != null && oldLineSpheres != null)
		{
			GPLineSphere[] data3 = oldLineSpheres.Data;
			GPLineSphere[] data4 = lineSpheres.Data;
			if (!flag2)
			{
				lineSphereCopyKernel.Dispatch();
			}
			for (int j = 0; j < lineSphereColliders.Count; j++)
			{
				CapsuleLineSphereCollider capsuleLineSphereCollider = lineSphereColliders[j];
				data4[j].PositionA = capsuleLineSphereCollider.WorldA;
				data4[j].PositionB = capsuleLineSphereCollider.WorldB;
				data4[j].RadiusA = capsuleLineSphereCollider.WorldRadiusA;
				data4[j].RadiusB = capsuleLineSphereCollider.WorldRadiusB;
				data4[j].Friction = capsuleLineSphereCollider.friction;
			}
			lineSpheres.PushData();
			if (flag2)
			{
				lineSphereCopyKernel.Dispatch();
			}
			lineSphereProcessKernel.Dispatch();
		}
		if (planes == null || planes.Count != planeColliders.Count)
		{
			if (planes != null)
			{
				planes.Dispose();
			}
			if (planeColliders.Count > 0)
			{
				planes = new GpuBuffer<Vector4>(planeColliders.Count, 16);
			}
			else
			{
				planes = null;
			}
		}
		if (planes != null)
		{
			Vector4[] data5 = planes.Data;
			for (int k = 0; k < planeColliders.Count; k++)
			{
				PlaneCollider planeCollider = planeColliders[k];
				data5[k] = planeCollider.GetWorldData();
			}
			planes.PushData();
		}
	}

	public void RegSphereCollider(GpuSphereCollider sphereCollider)
	{
		sphereColliders.Add(sphereCollider);
	}

	public void DeregSphereCollider(GpuSphereCollider sphereCollider)
	{
		sphereColliders.Remove(sphereCollider);
	}

	public void RegLineSphereCollider(CapsuleLineSphereCollider lineSphereCollider)
	{
		lineSphereColliders.Add(lineSphereCollider);
	}

	public void DeregLineSphereCollider(CapsuleLineSphereCollider lineSphereCollider)
	{
		lineSphereColliders.Remove(lineSphereCollider);
	}

	public void RegPlaneCollider(PlaneCollider planeCollider)
	{
		planeColliders.Add(planeCollider);
	}

	public void DeregPlaneCollider(PlaneCollider planeCollider)
	{
		planeColliders.Remove(planeCollider);
	}

	public void RegConsumer(GPUCollidersConsumer consumer)
	{
		consumers.Add(consumer);
	}

	public void DeregConsumer(GPUCollidersConsumer consumer)
	{
		consumers.Remove(consumer);
	}

	protected void ComputeGrabSpheres()
	{
		if (grabSpheresBuff == null || grabSpheresBuff.Count != grabSpheres.Count)
		{
			if (grabSpheres.Count > 0)
			{
				if (grabSpheresBuff != null)
				{
					grabSpheresBuff.Dispose();
				}
				grabSpheresBuff = new GpuBuffer<GPGrabSphere>(grabSpheres.Count, GPGrabSphere.Size());
				usingNullGrabSpheresBuff = false;
			}
			else if (!usingNullGrabSpheresBuff)
			{
				if (grabSpheresBuff != null)
				{
					grabSpheresBuff.Dispose();
				}
				grabSpheresBuff = new GpuBuffer<GPGrabSphere>(1, GPGrabSphere.Size());
				usingNullGrabSpheresBuff = true;
				GPGrabSphere[] data = grabSpheresBuff.Data;
				data[0].ID = -1;
				data[0].Position = Vector3.zero;
				data[0].Radius = 0f;
				data[0].GrabbedThisFrame = 0;
				grabSpheresBuff.PushData();
			}
		}
		if (grabSpheres.Count <= 0)
		{
			return;
		}
		GPGrabSphere[] data2 = grabSpheresBuff.Data;
		usingNullGrabSpheresBuff = false;
		for (int i = 0; i < grabSpheres.Count; i++)
		{
			GpuGrabSphere gpuGrabSphere = grabSpheres[i];
			data2[i].ID = gpuGrabSphere.id;
			data2[i].Position = gpuGrabSphere.transform.position;
			data2[i].Radius = gpuGrabSphere.WorldRadius;
			data2[i].GrabbedThisFrame = gpuGrabSphere.enabledThisFrame;
			if (gpuGrabSphere.frameCountdown == 0)
			{
				gpuGrabSphere.enabledThisFrame = 0;
			}
			else
			{
				gpuGrabSphere.frameCountdown--;
			}
		}
		grabSpheresBuff.PushData();
	}

	public void RegGrabSphere(GpuGrabSphere gs)
	{
		while (grabSphereIDs.ContainsKey(uidcount))
		{
			uidcount++;
			if (uidcount > 10000000)
			{
				uidcount = 0;
			}
		}
		grabSphereIDs.Add(uidcount, true);
		gs.id = uidcount;
		gs.enabledThisFrame = 1;
		grabSpheres.Add(gs);
	}

	public void DeregGrabSphere(GpuGrabSphere gs)
	{
		grabSphereIDs.Remove(gs.id);
		grabSpheres.Remove(gs);
	}

	protected GpuBuffer<GPLineSphere> ComputeEditCapsule(GpuBuffer<GPLineSphere> buffer, List<GpuEditCapsule> capsuleList)
	{
		GpuBuffer<GPLineSphere> gpuBuffer = buffer;
		if (gpuBuffer == null || gpuBuffer.Count != capsuleList.Count)
		{
			gpuBuffer?.Dispose();
			gpuBuffer = ((capsuleList.Count <= 0) ? null : new GpuBuffer<GPLineSphere>(capsuleList.Count, GPLineSphere.Size()));
		}
		if (capsuleList.Count > 0)
		{
			GPLineSphere[] data = gpuBuffer.Data;
			for (int i = 0; i < capsuleList.Count; i++)
			{
				GpuEditCapsule gpuEditCapsule = capsuleList[i];
				gpuEditCapsule.UpdateData();
				data[i].PositionA = gpuEditCapsule.WorldA;
				data[i].PositionB = gpuEditCapsule.WorldB;
				data[i].RadiusA = gpuEditCapsule.WorldRadiusA;
				data[i].RadiusB = gpuEditCapsule.WorldRadiusB;
				data[i].Friction = gpuEditCapsule.strength;
			}
			gpuBuffer.PushData();
		}
		return gpuBuffer;
	}

	protected void ComputeCutCapsules()
	{
		cutCapsulesBuff = ComputeEditCapsule(cutCapsulesBuff, cutCapsules);
	}

	public void RegCutCapsule(GpuEditCapsule gs)
	{
		cutCapsules.Add(gs);
	}

	public void DeregCutCapsule(GpuEditCapsule gs)
	{
		cutCapsules.Remove(gs);
	}

	protected void ComputeGrowCapsules()
	{
		growCapsulesBuff = ComputeEditCapsule(growCapsulesBuff, growCapsules);
	}

	public void RegGrowCapsule(GpuEditCapsule gs)
	{
		growCapsules.Add(gs);
	}

	public void DeregGrowCapsule(GpuEditCapsule gs)
	{
		growCapsules.Remove(gs);
	}

	protected void ComputeHoldCapsules()
	{
		holdCapsulesBuff = ComputeEditCapsule(holdCapsulesBuff, holdCapsules);
	}

	public void RegHoldCapsule(GpuEditCapsule gs)
	{
		holdCapsules.Add(gs);
	}

	public void DeregHoldCapsule(GpuEditCapsule gs)
	{
		holdCapsules.Remove(gs);
	}

	protected void ComputeGrabCapsules()
	{
		if (grabCapsulesBuff == null || grabCapsulesBuff.Count != grabCapsules.Count)
		{
			if (grabCapsulesBuff != null)
			{
				grabCapsulesBuff.Dispose();
			}
			if (grabCapsules.Count > 0)
			{
				grabCapsulesBuff = new GpuBuffer<GPLineSphereWithMatrixDelta>(grabCapsules.Count, GPLineSphereWithMatrixDelta.Size());
			}
			else
			{
				grabCapsulesBuff = null;
			}
		}
		if (grabCapsules.Count > 0)
		{
			GPLineSphereWithMatrixDelta[] data = grabCapsulesBuff.Data;
			for (int i = 0; i < grabCapsules.Count; i++)
			{
				GpuGrabCapsule gpuGrabCapsule = grabCapsules[i] as GpuGrabCapsule;
				gpuGrabCapsule.UpdateData();
				data[i].PositionA = gpuGrabCapsule.WorldA;
				data[i].PositionB = gpuGrabCapsule.WorldB;
				data[i].RadiusA = gpuGrabCapsule.WorldRadiusA;
				data[i].RadiusB = gpuGrabCapsule.WorldRadiusB;
				data[i].Friction = gpuGrabCapsule.strength;
				data[i].ChangeMatrix = gpuGrabCapsule.changeMatrix;
			}
			grabCapsulesBuff.PushData();
		}
	}

	public void RegGrabCapsule(GpuEditCapsule gs)
	{
		grabCapsules.Add(gs);
	}

	public void DeregGrabCapsule(GpuEditCapsule gs)
	{
		grabCapsules.Remove(gs);
	}

	protected void ComputePushCapsules()
	{
		pushCapsulesBuff = ComputeEditCapsule(pushCapsulesBuff, pushCapsules);
	}

	public void RegPushCapsule(GpuEditCapsule gs)
	{
		pushCapsules.Add(gs);
	}

	public void DeregPushCapsule(GpuEditCapsule gs)
	{
		pushCapsules.Remove(gs);
	}

	protected void ComputePullCapsules()
	{
		pullCapsulesBuff = ComputeEditCapsule(pullCapsulesBuff, pullCapsules);
	}

	public void RegPullCapsule(GpuEditCapsule gs)
	{
		pullCapsules.Add(gs);
	}

	public void DeregPullCapsule(GpuEditCapsule gs)
	{
		pullCapsules.Remove(gs);
	}

	protected void ComputeBrushCapsules()
	{
		if (brushCapsulesBuff == null || brushCapsulesBuff.Count != brushCapsules.Count)
		{
			if (brushCapsulesBuff != null)
			{
				brushCapsulesBuff.Dispose();
			}
			if (brushCapsules.Count > 0)
			{
				brushCapsulesBuff = new GpuBuffer<GPLineSphereWithDelta>(brushCapsules.Count, GPLineSphereWithDelta.Size());
			}
			else
			{
				brushCapsulesBuff = null;
			}
		}
		if (brushCapsules.Count > 0)
		{
			GPLineSphereWithDelta[] data = brushCapsulesBuff.Data;
			for (int i = 0; i < brushCapsules.Count; i++)
			{
				GpuEditCapsule gpuEditCapsule = brushCapsules[i];
				gpuEditCapsule.UpdateData();
				Vector3 positionA = data[i].PositionA;
				Vector3 positionB = data[i].PositionB;
				data[i].PositionA = gpuEditCapsule.WorldA;
				data[i].PositionB = gpuEditCapsule.WorldB;
				data[i].DeltaA = data[i].PositionA - positionA;
				data[i].DeltaB = data[i].PositionB - positionB;
				data[i].Delta = (data[i].DeltaA + data[i].DeltaB) * 0.5f;
				data[i].RadiusA = gpuEditCapsule.WorldRadiusA;
				data[i].RadiusB = gpuEditCapsule.WorldRadiusB;
				data[i].Friction = gpuEditCapsule.strength;
			}
			brushCapsulesBuff.PushData();
		}
	}

	public void RegBrushCapsule(GpuEditCapsule gs)
	{
		brushCapsules.Add(gs);
	}

	public void DeregBrushCapsule(GpuEditCapsule gs)
	{
		brushCapsules.Remove(gs);
	}

	protected void ComputeRigidityIncreaseCapsules()
	{
		rigidityIncreaseCapsulesBuff = ComputeEditCapsule(rigidityIncreaseCapsulesBuff, rigidityIncreaseCapsules);
	}

	public void RegRigidityIncreaseCapsule(GpuEditCapsule gs)
	{
		rigidityIncreaseCapsules.Add(gs);
	}

	public void DeregRigidityIncreaseCapsule(GpuEditCapsule gs)
	{
		rigidityIncreaseCapsules.Remove(gs);
	}

	protected void ComputeRigidityDecreaseCapsules()
	{
		rigidityDecreaseCapsulesBuff = ComputeEditCapsule(rigidityDecreaseCapsulesBuff, rigidityDecreaseCapsules);
	}

	public void RegRigidityDecreaseCapsule(GpuEditCapsule gs)
	{
		rigidityDecreaseCapsules.Add(gs);
	}

	public void DeregRigidityDecreaseCapsule(GpuEditCapsule gs)
	{
		rigidityDecreaseCapsules.Remove(gs);
	}

	protected void ComputeRigiditySetCapsules()
	{
		rigiditySetCapsulesBuff = ComputeEditCapsule(rigiditySetCapsulesBuff, rigiditySetCapsules);
	}

	public void RegRigiditySetCapsule(GpuEditCapsule gs)
	{
		rigiditySetCapsules.Add(gs);
	}

	public void DeregRigiditySetCapsule(GpuEditCapsule gs)
	{
		rigiditySetCapsules.Remove(gs);
	}

	public void Init()
	{
		if (!_wasInit)
		{
			_wasInit = true;
			singleton = this;
			sphereColliders = new List<GpuSphereCollider>();
			lineSphereColliders = new List<CapsuleLineSphereCollider>();
			planeColliders = new List<PlaneCollider>();
			lineSphereCopyKernel = new ParticleLineSphereCopyKernel();
			sphereCopyKernel = new ParticleSphereCopyKernel();
			lineSphereProcessKernel = new ParticleLineSphereProcessKernel();
			lineSphereProcessKernel.CollisionPrediction = new GpuValue<float>(0f);
			sphereProcessKernel = new ParticleSphereProcessKernel();
			sphereProcessKernel.CollisionPrediction = new GpuValue<float>(0f);
			consumers = new List<GPUCollidersConsumer>();
			grabSpheres = new List<GpuGrabSphere>();
			grabSphereIDs = new Dictionary<int, bool>();
			cutCapsules = new List<GpuEditCapsule>();
			growCapsules = new List<GpuEditCapsule>();
			pushCapsules = new List<GpuEditCapsule>();
			pullCapsules = new List<GpuEditCapsule>();
			brushCapsules = new List<GpuEditCapsule>();
			holdCapsules = new List<GpuEditCapsule>();
			grabCapsules = new List<GpuEditCapsule>();
			rigidityIncreaseCapsules = new List<GpuEditCapsule>();
			rigidityDecreaseCapsules = new List<GpuEditCapsule>();
			rigiditySetCapsules = new List<GpuEditCapsule>();
		}
	}

	private void Awake()
	{
		Init();
	}

	private void FixedUpdate()
	{
		fixedDispatchCount++;
		if (Time.fixedDeltaTime > 0.02f)
		{
			_collisionPrediction = 0f;
		}
		else
		{
			_collisionPrediction = Mathf.Clamp01(1f - Time.timeScale);
		}
		sphereProcessKernel.CollisionPrediction.Value = _collisionPrediction;
		lineSphereProcessKernel.CollisionPrediction.Value = _collisionPrediction;
		ComputeColliders();
		ComputeGrabSpheres();
		ComputeCutCapsules();
		ComputeGrowCapsules();
		ComputeHoldCapsules();
		ComputeGrabCapsules();
		ComputePushCapsules();
		ComputePullCapsules();
		ComputeBrushCapsules();
		ComputeRigidityIncreaseCapsules();
		ComputeRigidityDecreaseCapsules();
		ComputeRigiditySetCapsules();
	}

	private void Update()
	{
		fixedDispatchCount = 0;
	}

	private void OnDestroy()
	{
		if (spheres != null)
		{
			spheres.Dispose();
		}
		if (oldSpheres != null)
		{
			oldSpheres.Dispose();
		}
		if (processedSpheres != null)
		{
			processedSpheres.Dispose();
		}
		if (lineSpheres != null)
		{
			lineSpheres.Dispose();
		}
		if (oldLineSpheres != null)
		{
			oldLineSpheres.Dispose();
		}
		if (processedLineSpheres != null)
		{
			processedLineSpheres.Dispose();
		}
		if (planes != null)
		{
			planes.Dispose();
		}
		if (grabSpheresBuff != null)
		{
			grabSpheresBuff.Dispose();
		}
		if (cutCapsulesBuff != null)
		{
			cutCapsulesBuff.Dispose();
		}
		if (growCapsulesBuff != null)
		{
			growCapsulesBuff.Dispose();
		}
		if (holdCapsulesBuff != null)
		{
			holdCapsulesBuff.Dispose();
		}
		if (grabCapsulesBuff != null)
		{
			grabCapsulesBuff.Dispose();
		}
		if (pushCapsulesBuff != null)
		{
			pushCapsulesBuff.Dispose();
		}
		if (pullCapsulesBuff != null)
		{
			pullCapsulesBuff.Dispose();
		}
		if (brushCapsulesBuff != null)
		{
			brushCapsulesBuff.Dispose();
		}
		if (rigidityIncreaseCapsulesBuff != null)
		{
			rigidityIncreaseCapsulesBuff.Dispose();
		}
		if (rigidityDecreaseCapsulesBuff != null)
		{
			rigidityDecreaseCapsulesBuff.Dispose();
		}
		if (rigiditySetCapsulesBuff != null)
		{
			rigiditySetCapsulesBuff.Dispose();
		}
	}
}
