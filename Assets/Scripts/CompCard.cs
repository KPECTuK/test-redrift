using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CompCard : MonoBehaviour, IComponentCard
{
	[SerializeField]
	private TextMeshProUGUI _textTitle;
	[SerializeField]
	private TextMeshProUGUI _textDescription;
	[SerializeField]
	private TextMeshProUGUI _textStatusAttack;
	[SerializeField]
	private TextMeshProUGUI _textStatusHealth;
	[SerializeField]
	private TextMeshProUGUI _textStatusMana;
	[SerializeField]
	private Image _imageArt;

	public readonly Stack<ILayer> Placement = new Stack<ILayer>();

	public StateCounter CounterAttack { get; private set; }
	public StateCounter CounterHealth { get; private set; }
	public StateCounter CounterMana { get; private set; }
	private StateCounter[] _counters;

	private Coroutine _currentCoroutine;
	private Texture2D _textureArt;
	//
	private Transform _transform;
	private RectTransform _rectTransform;

	public CompHand ControllerCardLayout { get; private set; }

	private void Awake()
	{
		CounterAttack = new StateCounter(_textStatusAttack);
		CounterHealth = new StateCounter(_textStatusHealth);
		CounterMana = new StateCounter(_textStatusMana);
		_counters = new[] { CounterAttack, CounterHealth, CounterMana };
		//
		_currentCoroutine = StartCoroutine(LoadArtRandom());
		//
		_transform = transform;
		_rectTransform = GetComponent<RectTransform>();
		ControllerCardLayout = GetComponentInParent<CompHand>();
	}

	private void OnDestroy()
	{
		if(!ReferenceEquals(null, _currentCoroutine))
		{
			StopCoroutine(_currentCoroutine);
			_currentCoroutine = null;
		}

		// if(!ReferenceEquals(null, _imageArt))
		// {
		// 	Resources.UnloadAsset(_imageArt);
		// 	_imageArt = null;
		// }
	}

	private void LateUpdate()
	{
		using(var enumerator = Placement.GetEnumerator())
		{
			var acc = new Origin();
			while(enumerator.MoveNext())
			{
				if(!enumerator.Current.Eval(this, ref acc))
				{
					break;
				}
			}

			_rectTransform.position = acc.Position;
			_rectTransform.rotation = acc.Orient;
			_rectTransform.localScale = Vector3.one * acc.UniScale;
		}

		for(var index = 0; index < _counters.Length; index++)
		{
			_counters[index].ViewUpdate();
		}
	}

	private void OnMouseUpAsButton()
	{
		ControllerCardLayout.CardFocus(name);
	}

	private void OnMouseDrag()
	{
		ControllerCardLayout.CardDrag(name);
	}

	private IEnumerator LoadArtRandom()
	{
		var request = UnityWebRequestTexture.GetTexture("https://picsum.photos/200/200");

		yield return request.SendWebRequest();

		if(request.result != UnityWebRequest.Result.Success)
		{
			$"error loading art texture: {request.error}".LogError();
		}
		else
		{
			_textureArt = DownloadHandlerTexture.GetContent(request);
			;
			_imageArt.sprite = Sprite.Create(
				_textureArt,
				new Rect(0.0f, 0.0f, _textureArt.width, _textureArt.height),
				new Vector2(0.5f, 0.5f),
				100.0f);
		}

		_currentCoroutine = null;
	}

	public void SetTargetRandom()
	{
		var index = UnityEngine.Random.Range(0, _counters.Length);
		_counters[index].TargetSelect();
	}

	public void SetTargetAttack()
	{
		CounterAttack.TargetSelect();
	}

	public void SetTargetHealth()
	{
		CounterHealth.TargetSelect();
	}

	public void SetTargetMana()
	{
		CounterMana.TargetSelect();
	}
}

public class StateCounter
{
	private int _current;
	private int _target;
	private float _updateTimestamp;

	private readonly TextMeshProUGUI _comp;

	public int Value => _current - 2;

	public StateCounter(TextMeshProUGUI comp)
	{
		_comp = comp;
		TargetSelect();
		_current = _target;
		ViewSet();
	}

	public void TargetSelect()
	{
		_target = UnityEngine.Random.Range(3, 12);
	}

	public void ViewUpdate()
	{
		if(_current != _target)
		{
			_updateTimestamp -= Time.deltaTime;
			if(_updateTimestamp < 0f)
			{
				_updateTimestamp = 1f;
				_current += _target > _current ? 1 : -1;
				ViewSet();
			}
		}
	}

	public void ViewSet()
	{
		_comp.text = $"{Value}";
	}
}

public interface IComponentCard
{
	CompHand ControllerCardLayout { get; }
}

public interface ILayer
{
	bool Eval(IComponentCard compImmutable, ref Origin current);
}

public class LayerFocus : ILayer
{
	private const float SPEED_SCALE_F = .03f;
	//
	private const float GAP_LOW_F = .1f;
	private const float GAP_HIGH_F = .5f;
	private const float SCALE_LOW_F = .4f;
	private const float SCALE_HIGH_F = .5f;

	private bool _got;
	private float _target;

	public float SocketSize => Mathf.SmoothStep(GAP_LOW_F, GAP_HIGH_F, _target);

	public void Reset(bool got)
	{
		_got = got;
	}

	public bool Eval(IComponentCard compImmutable, ref Origin current)
	{
		_target += (_got ? 1f : -1f) * SPEED_SCALE_F;
		_target = Mathf.Clamp01(_target);
		current.UniScale = Mathf.SmoothStep(SCALE_LOW_F, SCALE_HIGH_F, _target);
		return true;
	}
}

public class LayerArrange : ILayer
{
	private float _length;
	private float _offset;

	public void Reset(float length, float offset)
	{
		_length = length;
		_offset = offset;
	}

	public bool Eval(IComponentCard compImmutable, ref Origin current)
	{
		var curveHand = compImmutable.ControllerCardLayout.CurveHand;
		var range = 80f * curveHand.Long / 100f;
		var side = 10f * curveHand.Long / 100f;
		var curveDistance = side + range * _offset / _length;
		if(curveHand.GetPosition(curveDistance, out var position, out var tangent))
		{
			current.Position = position;
			current.Orient = Quaternion.FromToRotation(Vector3.right, -tangent);

			return true;
		}

		throw new Exception("cards no fit");
	}
}

public static class ExtensionsUtility
{
	private const float SCALE_GLOBAL_F = .03f;

	public static void LogDebug(this string message, UnityEngine.Object source = null)
	{
		Debug.Log(message, source);
	}

	public static void LogError(this string message, UnityEngine.Object source = null)
	{
		Debug.LogError(message, source);
	}

	public static void DrawVectorAt(this Vector2 vector, Vector2 position, float scale = 1f)
	{
		using(new WithColor(Color.blue))
		{
			Gizmos.DrawLine(position, position + vector.normalized * (scale * SCALE_GLOBAL_F));
		}
	}

	public static void DrawAsPoint(this Vector3 position, float scale = 1f)
	{
		DrawAsPoint((Vector2)position, Quaternion.identity, scale);
	}

	public static void DrawAsPoint(this Vector3 position, Quaternion orient, float scale = 1f)
	{
		DrawAsPoint((Vector2)position, orient, scale);
	}

	public static void DrawAsPoint(this Vector2 position, float scale = 1f)
	{
		DrawAsPoint(position, Quaternion.identity, scale);
	}

	public static void DrawAsPoint(this Vector2 position, Quaternion orient, float scale = 1f, float duration = .5f)
	{
		if(Application.isPlaying)
		{
			scale *= SCALE_GLOBAL_F;
			var up = (Vector2)(orient * Vector3.up);
			var down = (Vector2)(orient * Vector3.down);
			var left = (Vector2)(orient * Vector3.left);
			var right = (Vector2)(orient * Vector3.right);
			Debug.DrawLine(position, position + down * scale, Color.black, duration);
			Debug.DrawLine(position, position + left * scale, Color.black, duration);
			Debug.DrawLine(position, position + right * scale, Color.red, duration);
			Debug.DrawLine(position, position + up * scale, Color.green, duration);
		}
		else
		{
			using(new WithColor(Color.black))
			{
				scale *= SCALE_GLOBAL_F;
				var up = (Vector2)(orient * Vector3.up);
				var down = (Vector2)(orient * Vector3.down);
				var left = (Vector2)(orient * Vector3.left);
				var right = (Vector2)(orient * Vector3.right);
				Gizmos.DrawLine(position, position + down * scale);
				Gizmos.DrawLine(position, position + left * scale);
				Gizmos.color = Color.red;
				Gizmos.DrawLine(position, position + right * scale);
				Gizmos.color = Color.green;
				Gizmos.DrawLine(position, position + up * scale);
			}
		}
	}

	public static void DrawAsKnot(this Vector3 position, float scale = 1f)
	{
		DrawAsKnot((Vector2)position, Quaternion.identity, scale);
	}

	public static void DrawAsKnot(this Vector3 position, Quaternion orient, float scale = 1f)
	{
		DrawAsKnot((Vector2)position, orient, scale);
	}

	public static void DrawAsKnot(this Vector2 position, float scale = 1f)
	{
		DrawAsKnot(position, Quaternion.identity, scale);
	}

	public static void DrawAsKnot(this Vector2 position, Quaternion orient, float scale = 1f)
	{
		using(new WithColor(Color.gray))
		{
			scale *= SCALE_GLOBAL_F * .5f;
			var p0 = (Vector2)(orient * new Vector3(1f, 1f) * scale) + position;
			var p1 = (Vector2)(orient * new Vector3(1f, -1f) * scale) + position;
			var p2 = (Vector2)(orient * new Vector3(-1f, -1f) * scale) + position;
			var p3 = (Vector2)(orient * new Vector3(-1f, 1f) * scale) + position;
			Gizmos.DrawLine(p0, p1);
			Gizmos.DrawLine(p1, p2);
			Gizmos.DrawLine(p2, p3);
			Gizmos.DrawLine(p3, p0);
		}
	}
}
