using UnityEngine;

namespace Leap.Unity.Animation.Internal
{
	public struct MaterialSelector
	{
		private class MaterialColorInterpolator : ColorInterpolatorBase<MaterialPropertyKey>
		{
			public override bool isValid => _target.material != null;

			public override void Interpolate(float percent)
			{
				_target.material.SetColor(_target.propertyId, _a + _b * percent);
			}

			public override void Dispose()
			{
				_target.material = null;
				Pool<MaterialColorInterpolator>.Recycle(this);
			}
		}

		private class MaterialGradientInterpolator : GradientInterpolatorBase
		{
			private MaterialPropertyKey _matPropKey;

			public override bool isValid => _matPropKey.material != null;

			public MaterialGradientInterpolator Init(Gradient gradient, MaterialPropertyKey matPropKey)
			{
				_matPropKey = matPropKey;
				Init(gradient);
				return this;
			}

			public override void Interpolate(float percent)
			{
				_matPropKey.material.SetColor(_matPropKey.propertyId, _gradient.Evaluate(percent));
			}
		}

		private class MaterialRGBInterpolator : Vector3InterpolatorBase<MaterialPropertyKey>
		{
			public override bool isValid => _target.material != null;

			public override void Interpolate(float percent)
			{
				float a = _target.material.GetColor(_target.propertyId).a;
				Color value = (Vector4)(_a + _b * percent);
				value.a = a;
				_target.material.SetColor(_target.propertyId, value);
			}

			public override void Dispose()
			{
				_target.material = null;
				Pool<MaterialRGBInterpolator>.Recycle(this);
			}
		}

		private class MaterialAlphaInterpolator : FloatInterpolatorBase<MaterialPropertyKey>
		{
			public override bool isValid => _target.material != null;

			public override void Interpolate(float percent)
			{
				Color color = _target.material.GetColor(_target.propertyId);
				color.a = Mathf.Lerp(_a, _b, percent);
				_target.material.SetColor(_target.propertyId, color);
			}

			public override void Dispose()
			{
				_target.material = null;
				Pool<MaterialAlphaInterpolator>.Recycle(this);
			}
		}

		private class MaterialFloatInterpolator : FloatInterpolatorBase<MaterialPropertyKey>
		{
			public override bool isValid => _target.material != null;

			public override void Interpolate(float percent)
			{
				_target.material.SetFloat(_target.propertyId, _a + _b * percent);
			}

			public override void Dispose()
			{
				_target.material = null;
				Pool<MaterialFloatInterpolator>.Recycle(this);
			}
		}

		private class MaterialVectorInterpolator : Vector4InterpolatorBase<MaterialPropertyKey>
		{
			public override bool isValid => _target.material != null;

			public override void Interpolate(float percent)
			{
				_target.material.SetVector(_target.propertyId, _a + _b * percent);
			}

			public override void Dispose()
			{
				_target.material = null;
				Pool<MaterialVectorInterpolator>.Recycle(this);
			}
		}

		private class MaterialInterpolator : InterpolatorBase<Material, Material>
		{
			public override float length => 1f;

			public override bool isValid => _target != null;

			public override void Interpolate(float percent)
			{
				_target.Lerp(_a, _b, percent);
			}

			public override void Dispose()
			{
				_target = null;
				_a = null;
				_b = null;
				Pool<MaterialInterpolator>.Recycle(this);
			}
		}

		private struct MaterialPropertyKey
		{
			public Material material;

			public int propertyId;

			public MaterialPropertyKey(Material material, int propertyId)
			{
				this.material = material;
				this.propertyId = propertyId;
			}
		}

		private Material _target;

		private Tween _tween;

		public MaterialSelector(Material target, Tween tween)
		{
			_target = target;
			_tween = tween;
		}

		public Tween Color(Color a, Color b, int propertyId)
		{
			return _tween.AddInterpolator(Pool<MaterialColorInterpolator>.Spawn().Init(a, b, new MaterialPropertyKey(_target, propertyId)));
		}

		public Tween Color(Color a, Color b, string propertyName = "_Color")
		{
			return Color(a, b, Shader.PropertyToID(propertyName));
		}

		public Tween Color(Gradient gradient, int propertyId)
		{
			return _tween.AddInterpolator(Pool<MaterialGradientInterpolator>.Spawn().Init(gradient, new MaterialPropertyKey(_target, propertyId)));
		}

		public Tween Color(Gradient gradient, string propertyName = "_Color")
		{
			return Color(gradient, Shader.PropertyToID(propertyName));
		}

		public Tween ToColor(Color b, int propertyId)
		{
			return _tween.AddInterpolator(Pool<MaterialColorInterpolator>.Spawn().Init(_target.GetColor(propertyId), b, new MaterialPropertyKey(_target, propertyId)));
		}

		public Tween ToColor(Color b, string propertyName = "_Color")
		{
			return ToColor(b, Shader.PropertyToID(propertyName));
		}

		public Tween RGB(Color a, Color b, int propertyId)
		{
			return _tween.AddInterpolator(Pool<MaterialRGBInterpolator>.Spawn().Init((Vector4)a, (Vector4)b, new MaterialPropertyKey(_target, propertyId)));
		}

		public Tween RGB(Color a, Color b, string propertyName = "_Color")
		{
			return RGB(a, b, Shader.PropertyToID(propertyName));
		}

		public Tween ToRGB(Color b, int propertyId)
		{
			return _tween.AddInterpolator(Pool<MaterialRGBInterpolator>.Spawn().Init((Vector4)_target.GetColor(propertyId), (Vector4)b, new MaterialPropertyKey(_target, propertyId)));
		}

		public Tween ToRGB(Color b, string propertyName = "_Color")
		{
			return ToRGB(b, Shader.PropertyToID(propertyName));
		}

		public Tween Alpha(float a, float b, int propertyId)
		{
			return _tween.AddInterpolator(Pool<MaterialAlphaInterpolator>.Spawn().Init(a, b, new MaterialPropertyKey(_target, propertyId)));
		}

		public Tween Alpha(float a, float b, string propertyName = "_Color")
		{
			return Alpha(a, b, Shader.PropertyToID(propertyName));
		}

		public Tween ToAlpha(float b, int propertyId)
		{
			return _tween.AddInterpolator(Pool<MaterialAlphaInterpolator>.Spawn().Init(_target.GetColor(propertyId).a, b, new MaterialPropertyKey(_target, propertyId)));
		}

		public Tween ToAlpha(float b, string propertyName = "_Color")
		{
			return ToAlpha(b, Shader.PropertyToID(propertyName));
		}

		public Tween Float(float a, float b, int propertyId)
		{
			return _tween.AddInterpolator(Pool<MaterialFloatInterpolator>.Spawn().Init(a, b, new MaterialPropertyKey(_target, propertyId)));
		}

		public Tween Float(float a, float b, string propertyName)
		{
			return Float(a, b, Shader.PropertyToID(propertyName));
		}

		public Tween ToFloat(float b, int propertyId)
		{
			return _tween.AddInterpolator(Pool<MaterialFloatInterpolator>.Spawn().Init(_target.GetFloat(propertyId), b, new MaterialPropertyKey(_target, propertyId)));
		}

		public Tween ToFloat(float b, string propertyName)
		{
			return ToFloat(b, Shader.PropertyToID(propertyName));
		}

		public Tween Vector(Vector4 a, Vector4 b, int propertyId)
		{
			return _tween.AddInterpolator(Pool<MaterialVectorInterpolator>.Spawn().Init(a, b, new MaterialPropertyKey(_target, propertyId)));
		}

		public Tween Vector(Vector4 a, Vector4 b, string propertyName)
		{
			return Vector(a, b, Shader.PropertyToID(propertyName));
		}

		public Tween Vector(Vector3 a, Vector3 b, int propertyId)
		{
			return _tween.AddInterpolator(Pool<MaterialVectorInterpolator>.Spawn().Init(a, b, new MaterialPropertyKey(_target, propertyId)));
		}

		public Tween Vector(Vector3 a, Vector3 b, string propertyName)
		{
			return Vector(a, b, Shader.PropertyToID(propertyName));
		}

		public Tween ToVector(Vector4 b, int propertyId)
		{
			return _tween.AddInterpolator(Pool<MaterialVectorInterpolator>.Spawn().Init(_target.GetVector(propertyId), b, new MaterialPropertyKey(_target, propertyId)));
		}

		public Tween ToVector(Vector4 b, string propertyName)
		{
			return ToVector(b, Shader.PropertyToID(propertyName));
		}

		public Tween ToVector(Vector3 b, int propertyId)
		{
			return _tween.AddInterpolator(Pool<MaterialVectorInterpolator>.Spawn().Init(_target.GetVector(propertyId), b, new MaterialPropertyKey(_target, propertyId)));
		}

		public Tween ToVector(Vector3 b, string propertyName)
		{
			return ToVector(b, Shader.PropertyToID(propertyName));
		}

		public Tween Material(Material a, Material b)
		{
			return _tween.AddInterpolator(Pool<MaterialInterpolator>.Spawn().Init(a, b, _target));
		}
	}
}
