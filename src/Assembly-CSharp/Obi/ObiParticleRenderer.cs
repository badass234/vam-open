using System;
using System.Collections.Generic;
using UnityEngine;

namespace Obi
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(ObiActor))]
	public class ObiParticleRenderer : MonoBehaviour
	{
		public bool render = true;

		public Color particleColor = Color.white;

		public float radiusScale = 1f;

		private ObiActor actor;

		private List<Mesh> meshes = new List<Mesh>();

		private Material material;

		private List<Vector3> vertices = new List<Vector3>();

		private List<Vector3> normals = new List<Vector3>();

		private List<Color> colors = new List<Color>();

		private List<int> triangles = new List<int>();

		private int particlesPerDrawcall;

		private int drawcallCount;

		private Vector3[] particleOffsets = new Vector3[4]
		{
			new Vector3(1f, 1f, 0f),
			new Vector3(-1f, 1f, 0f),
			new Vector3(-1f, -1f, 0f),
			new Vector3(1f, -1f, 0f)
		};

		public IEnumerable<Mesh> ParticleMeshes => meshes;

		public Material ParticleMaterial => material;

		public void Awake()
		{
			actor = GetComponent<ObiActor>();
		}

		public void OnEnable()
		{
			material = UnityEngine.Object.Instantiate(Resources.Load<Material>("ObiMaterials/Particle"));
			material.hideFlags = HideFlags.HideAndDontSave;
			if (actor != null && actor.Solver != null)
			{
				particlesPerDrawcall = 16250;
				drawcallCount = actor.positions.Length / particlesPerDrawcall + 1;
				particlesPerDrawcall = Mathf.Min(particlesPerDrawcall, actor.positions.Length);
				actor.Solver.RequireRenderablePositions();
				actor.Solver.OnFrameEnd += Actor_solver_OnFrameEnd;
			}
		}

		public void OnDisable()
		{
			if (actor != null && actor.Solver != null)
			{
				actor.Solver.RelinquishRenderablePositions();
				actor.Solver.OnFrameEnd -= Actor_solver_OnFrameEnd;
			}
			ClearMeshes();
			UnityEngine.Object.DestroyImmediate(material);
		}

		private void Actor_solver_OnFrameEnd(object sender, EventArgs e)
		{
			if (actor == null || !actor.InSolver || !actor.isActiveAndEnabled)
			{
				ClearMeshes();
				return;
			}
			ObiSolver solver = actor.Solver;
			if (drawcallCount != meshes.Count)
			{
				ClearMeshes();
				for (int i = 0; i < drawcallCount; i++)
				{
					Mesh mesh = new Mesh();
					mesh.name = "Particle imposters";
					mesh.hideFlags = HideFlags.HideAndDontSave;
					mesh.MarkDynamic();
					meshes.Add(mesh);
				}
			}
			for (int j = 0; j < drawcallCount; j++)
			{
				vertices.Clear();
				normals.Clear();
				colors.Clear();
				triangles.Clear();
				Color white = Color.white;
				int num = 0;
				for (int k = j * particlesPerDrawcall; k < (j + 1) * particlesPerDrawcall; k++)
				{
					if (actor.active[k])
					{
						AddParticle(color: (actor.colors == null || k >= actor.colors.Length) ? particleColor : (actor.colors[k] * particleColor), i: num, position: solver.renderablePositions[actor.particleIndices[k]], radius: actor.solidRadii[k] * radiusScale);
						num++;
					}
				}
				Apply(meshes[j]);
			}
			if (render)
			{
				DrawParticles();
			}
		}

		private void DrawParticles()
		{
			foreach (Mesh mesh in meshes)
			{
				Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, base.gameObject.layer);
			}
		}

		private void Apply(Mesh mesh)
		{
			mesh.Clear();
			mesh.SetVertices(vertices);
			mesh.SetNormals(normals);
			mesh.SetColors(colors);
			mesh.SetTriangles(triangles, 0, true);
		}

		private void ClearMeshes()
		{
			foreach (Mesh mesh in meshes)
			{
				UnityEngine.Object.DestroyImmediate(mesh);
			}
			meshes.Clear();
		}

		private void AddParticle(int i, Vector3 position, Color color, float radius)
		{
			int num = i * 4;
			int item = num + 1;
			int item2 = num + 2;
			int item3 = num + 3;
			vertices.Add(position);
			vertices.Add(position);
			vertices.Add(position);
			vertices.Add(position);
			particleOffsets[0].z = radius;
			particleOffsets[1].z = radius;
			particleOffsets[2].z = radius;
			particleOffsets[3].z = radius;
			normals.Add(particleOffsets[0]);
			normals.Add(particleOffsets[1]);
			normals.Add(particleOffsets[2]);
			normals.Add(particleOffsets[3]);
			colors.Add(color);
			colors.Add(color);
			colors.Add(color);
			colors.Add(color);
			triangles.Add(item2);
			triangles.Add(item);
			triangles.Add(num);
			triangles.Add(item3);
			triangles.Add(item2);
			triangles.Add(num);
		}
	}
}
