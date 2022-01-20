using System;
using UnityEngine;

public class CompCurve : MonoBehaviour, ICurve
{
	[Range(0f, 1f)]
	public float Alpha = 0.5f;

	private int KnotCount => transform.childCount;
	private int SegmentCount => KnotCount - 3;

	public float Long { get; private set; }

	[NonSerialized]
	public CurveSegment[] Segments;

	private void Awake()
	{
		BuildCurveData();
	}

	// private float _debigDistance = 0f;
	//
	// private void Update()
	// {
	// 	if(Segments.GetPosition(_debigDistance, out var position, out var tangent))
	// 	{
	// 		position.DrawAsPoint(Quaternion.FromToRotation(Vector3.up, tangent));
	// 		_debigDistance += .5f * Time.deltaTime;
	//
	// 	}
	// 	else
	// 	{
	// 		_debigDistance = 0f;
	// 	}
	// }

	private void BuildCurveData()
	{
		Segments = new CurveSegment[Mathf.Clamp(SegmentCount, 0, int.MaxValue)];
		for(var index = 0; index < Segments.Length; index++)
		{
			Segments[index] = GetSegment(index);
			Long += Segments[index].Long;
		}
	}

	private Vector2 GetKnot(int index)
	{
		return transform.GetChild(index).position;
	}

	private CurveSegment GetSegment(int index)
	{
		return new CurveSegment(
			GetKnot(index),
			GetKnot(index + 1),
			GetKnot(index + 2),
			GetKnot(index + 3),
			Alpha);
	}

	#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if(Application.isPlaying)
		{
			for(var index = 0; index < Segments.Length; index++)
			{
				DrawSegment(Segments[index]);
			}
		}
		else
		{
			for(var index = 0; index < SegmentCount; index++)
			{
				DrawSegment(GetSegment(index));
			}
		}

		foreach(Transform point in transform)
		{
			point.position.DrawAsKnot();
		}
	}

	private void DrawSegment(CurveSegment segment)
	{
		const float DIMM_F = .6f;
		var colorStart = new Color(1f * DIMM_F, .92f * DIMM_F, .016f * DIMM_F);
		var colorStop = Color.yellow;

		var pFrom = segment.P1_Seg;
		var step = 0f;
		while(true)
		{
			step += CurveSegment.LOD_STEP_F;
			if(step > CurveSegment.LOD_F)
			{
				break;
			}

			//! clamp
			var stepNormalized = step / CurveSegment.LOD_F;
			var pTo = segment.GetPoint(stepNormalized);
			var colorCurrent = Color.LerpUnclamped(colorStart, colorStop, stepNormalized);
			using(new WithColor(colorCurrent))
			{
				Gizmos.DrawLine(pFrom, pTo);
			}
			pFrom = pTo;
		}
	}
	#endif
}

public static class ExtensionsCurve
{
	private const float EPSILON_F = .000001f;
	private const float INITIAL_STEPS_F = CurveSegment.LOD_F;

	public static bool GetPosition(this ICurve source, float distance, out Vector2 position, out Vector2 tangent)
	{
		position = Vector2.zero;
		tangent = Vector2.zero;
		return source is CompCurve cast && cast.Segments.GetPosition(distance, out position, out tangent);
	}

	public static bool GetPosition(this CurveSegment[] segments, float distance, out Vector2 position, out Vector2 tangent)
	{
		var indexSegment = 0;
		while(indexSegment < segments.Length)
		{
			if(distance < segments[indexSegment].Long)
			{
				var pFrom = segments[indexSegment].P1_Seg;
				var segDistance = 0f;
				var paramDistance = 0f;
				var paramStep = 1f / INITIAL_STEPS_F;
				var counterBreak = 0;
				while(true)
				{
					if(++counterBreak > 100)
					{
						position = pFrom;
						tangent = segments[indexSegment].GetTangent(0f);
						return false;
					}

					var paramValue = paramDistance + paramStep;
					var pTo = segments[indexSegment].GetPoint(paramValue);
					var segStep = (pTo - pFrom).magnitude;
					var approx = distance - segDistance - segStep;

					if(approx < 0f)
					{
						paramStep /= 2f;
						continue;
					}

					if(approx < EPSILON_F)
					{
						// const float SCALE_F = 100000f;
						// $"[{counterBreak:000}] approx: {SCALE_F * approx:F5} step: {SCALE_F * step:F5}".LogDebug();

						position = pTo;
						tangent = segments[indexSegment].GetTangent(paramValue);
						return true;
					}

					pFrom = pTo;
					paramDistance += paramStep;
					segDistance += segStep;
				}
			}

			distance -= segments[indexSegment].Long;
			indexSegment++;
		}

		position = Vector2.zero;
		tangent = Vector2.zero;
		return false;
	}
}

public interface ICurve
{
	float Long { get; }
}

public struct CurveSegment
{
	public const float DELTA_F = .01f;
	public const float LOD_F = 32f;
	public const float LOD_STEP_F = 1f;

	// ReSharper disable MemberCanBePrivate.Global
	public readonly Vector2 P0_Tan;
	public readonly Vector2 P1_Seg;
	public readonly Vector2 P2_Seg;
	public readonly Vector2 P3_Tan;
	public readonly float Alpha;
	public readonly float Long;
	// ReSharper restore MemberCanBePrivate.Global

	public CurveSegment(
		Vector2 p0Tan,
		Vector2 p1Seg,
		Vector2 p2Seg,
		Vector2 p3Tan,
		float alpha)
	{
		P0_Tan = p0Tan;
		P1_Seg = p1Seg;
		P2_Seg = p2Seg;
		P3_Tan = p3Tan;
		Alpha = alpha;
		Long = 0;

		var pFrom = P1_Seg;
		var step = 0f;
		while(true)
		{
			step += LOD_STEP_F;
			if(step > LOD_F)
			{
				break;
			}

			//! clamp
			var pTo = GetPoint(step / LOD_F);
			Long += (pTo - pFrom).magnitude;
			pFrom = pTo;
		}
	}

	public Vector2 GetTangent(float t)
	{
		var p0 = GetPoint(t - DELTA_F);
		var p1 = GetPoint(t + DELTA_F);
		var result = (p1 - p0) * .5f;
		// $"tan: ({result.x:F8}, {result.y:F8})".LogDebug();
		return result.normalized;
	}

	public Vector2 GetPoint(float t)
	{
		// ReSharper disable once InconsistentNaming
		const float k0 = 0;
		var k1 = GetKnotInterval(P0_Tan, P1_Seg);
		var k2 = GetKnotInterval(P1_Seg, P2_Seg) + k1;
		var k3 = GetKnotInterval(P2_Seg, P3_Tan) + k2;

		var u = Mathf.LerpUnclamped(k1, k2, t);

		var a1 = Remap(k0, k1, P0_Tan, P1_Seg, u);
		var a2 = Remap(k1, k2, P1_Seg, P2_Seg, u);
		var a3 = Remap(k2, k3, P2_Seg, P3_Tan, u);

		var b1 = Remap(k0, k2, a1, a2, u);
		var b2 = Remap(k1, k3, a2, a3, u);

		var result = Remap(k1, k2, b1, b2, u);

		return result;
	}

	private static Vector2 Remap(float a, float b, Vector2 c, Vector2 d, float u)
	{
		return Vector2.LerpUnclamped(c, d, (u - a) / (b - a));
	}

	private float GetKnotInterval(Vector2 a, Vector2 b)
	{
		return Mathf.Pow(Vector2.SqrMagnitude(a - b), .5f * Alpha);
	}
}

public struct Origin
{
	public Vector2 Position;
	public Quaternion Orient;
	public float UniScale;
}

public struct WithColor : IDisposable
{
	private readonly Color _backup;
	private readonly bool _hadChanged;

	public WithColor(Color color)
	{
		_backup = Gizmos.color;
		Gizmos.color = color;
		_hadChanged = true;
	}

	public void Dispose()
	{
		if(_hadChanged)
		{
			Gizmos.color = _backup;
		}
	}
}
