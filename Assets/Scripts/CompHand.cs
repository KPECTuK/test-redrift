using UnityEngine;

public class CompHand : MonoBehaviour
{
	[SerializeField]
	private CompCurve _curveHand;

	private CompCard[] _cards;
	private LayerArrange[] _layerHandPos;
	private LayerFocus[] _layerHandFocus;
	private int _focusGotBy = -1;
	private int _indexCardAction = -1;

	public ICurve CurveHand => _curveHand;

	private void Awake()
	{
		var sizeCards = Random.Range(4, 7);
		_cards = new CompCard[sizeCards];
		_layerHandPos = new LayerArrange[sizeCards];
		_layerHandFocus = new LayerFocus[sizeCards];

		var prototype = Resources.Load<GameObject>("card_any");
		for(var index = 0; index < _cards.Length; index++)
		{
			_cards[index] = Instantiate(prototype, transform).GetComponent<CompCard>();
			_cards[index].transform.SetAsFirstSibling();

			var name = $"cc_{index:00}";
			string.Intern(name);
			_cards[index].name = name;

			_layerHandPos[index] = new LayerArrange();
			_layerHandFocus[index] = new LayerFocus();

			_cards[index].Placement.Push(_layerHandPos[index]);
			_cards[index].Placement.Push(_layerHandFocus[index]);
		}
	}

	private void LateUpdate()
	{
		// remove expelled
		var targetSize = _cards.Length;
		for(var index = 0; index < _cards.Length; index++)
		{
			if(_cards[index].CounterHealth.Value < 1)
			{
				Destroy(_cards[index].gameObject);

				_cards[index] = null;
				_layerHandPos[index] = null;
				_layerHandFocus[index] = null;

				targetSize--;
			}
		}

		if(targetSize != _cards.Length)
		{
			// resize controller pool
			var targetCards = new CompCard[targetSize];
			var targetLayerHandPos = new LayerArrange[targetSize];
			var targetLayerHandFocus = new LayerFocus[targetSize];

			var indexTarget = -1;
			for(var index = 0; index < _cards.Length; index++)
			{
				if(ReferenceEquals(null, _cards[index]))
				{
					continue;
				}

				++indexTarget;
				targetCards[indexTarget] = _cards[index];
				targetLayerHandPos[indexTarget] = _layerHandPos[index];
				targetLayerHandFocus[indexTarget] = _layerHandFocus[index];
			}

			_cards = targetCards;
			_layerHandPos = targetLayerHandPos;
			_layerHandFocus = targetLayerHandFocus;

			// reset controller: focus
			_focusGotBy = -1;
			for(var index = 0; index < _layerHandFocus.Length; index++)
			{
				_layerHandFocus[index].Reset(false);
				_cards[index].transform.SetAsLastSibling();
			}
		}

		// distribute size
		var length = 0f;
		for(var index = 0; index < _layerHandFocus.Length; index++)
		{
			length += _layerHandFocus[index].SocketSize;
		}

		var offset = 0f;
		for(var index = 0; index < _layerHandPos.Length; index++)
		{
			offset += _layerHandFocus[index].SocketSize * .5f;
			_layerHandPos[index].Reset(length, offset);
			offset += _layerHandFocus[index].SocketSize * .5f;
		}

		// sort depth
		if(_focusGotBy != -1)
		{
			var indexSide = 0;
			for(var index = 0; index < _cards.Length; index++)
			{
				var indexTop = _cards.Length - 1;
				
				var indexLeft = _focusGotBy + indexSide;
				if(Mathf.Clamp(indexLeft, 0, indexTop) == indexLeft)
				{
					_cards[indexLeft].transform.SetAsFirstSibling();
				}
				
				var indexRight = _focusGotBy - indexSide;
				if(Mathf.Clamp(indexRight, 0, indexTop) == indexRight)
				{
					_cards[indexRight].transform.SetAsFirstSibling();
				}

				indexSide++;
			}
		}
	}

	private int GetById(string id)
	{
		var index = _cards.Length;
		while(--index != -1)
		{
			if(_cards[index].name.Equals(id))
			{
				break;
			}
		}

		return index;
	}

	public void CardFocus(string id)
	{
		var indexFocus = GetById(id);
		if(_focusGotBy == indexFocus)
		{
			return;
		}

		_focusGotBy = indexFocus;
		for(var index = 0; index < _layerHandFocus.Length; index++)
		{
			_layerHandFocus[index].Reset(index == indexFocus);
		}
	}

	public void CardDrag(string id)
	{
		var indexFocus = GetById(id);
		if(_focusGotBy != indexFocus)
		{
			return;
		}


	}

	public void CardAction()
	{
		if(_cards.Length == 0)
		{
			return;
		}

		// left to right
		_indexCardAction = ++_indexCardAction % _cards.Length;
		_cards[_cards.Length - _indexCardAction - 1].SetTargetRandom();
	}
}
