using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;

namespace Wrj
{
    public class CardUI : DropTarget, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        [SerializeField]
        private Image cardRenderer;
        public UnityEvent StateChange;
        private PlayingCards.Card _card;
        public PlayingCards.Card Card
        {
            set
            {
                if (_card != null && _card != value)
                {
                    _allCards.Remove(_card);
                }
                _card = value;
                _allCards.Add(_card, this);
                UpdateImage();
            }
            get => _card;
        }
        public SolitaireSuitPile inSuitPile = null;
        public SolitaireColumn inColumn = null;
        public CardUI childCard = null;
        public bool IsParked => transform.parent == Solitaire.ParkingLot;

        private static Dictionary<PlayingCards.Card, CardUI> _allCards = new Dictionary<PlayingCards.Card, CardUI>();
        public static CardUI GetCardUI(PlayingCards.Card card)
        {
            if (_allCards != null && _allCards.ContainsKey(card))
            {
                CardUI cardUi = _allCards[card];
                cardUi.UpdateImage();
                return cardUi;
            }
            return null;
        }

        public void Park()
        {
            transform.SetParent(Solitaire.ParkingLot);
            gameObject.SetActive(false);
        }
        public void PlaceToParent(Transform parent)
        {
            transform.SetParent(parent);
            gameObject.SetActive(true);
        }

        private void Start()
        {
            PlayingCards.onCardLocationChanged += HandleCardMove;
        }

        private void HandleCardMove(PlayingCards.Card moved)
        {
            if (childCard == null) return;
            if (childCard == GetCardUI(moved))
            {
                childCard = null;
            }
        }
        public void UpdateImage()
        {
            cardRenderer.sprite = _card.Sprite;
        }
        private void OnDestroy()
        {
            _allCards.Remove(_card);
        }

        public static void DestroyAllCards()
        {
            if (_allCards == null) return;
            Dictionary<PlayingCards.Card, CardUI> allCardsCopy = new Dictionary<PlayingCards.Card, CardUI>(_allCards);
            foreach (var kvp in allCardsCopy)
            {
                Destroy(kvp.Value.gameObject);
            }
        }

        private static CardUI _draggingCard = null;
        private Vector2 initMousePos = Vector2.zero;
        private Vector2 initCardPos = Vector2.zero;
        private Transform initParent;
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_card.isFaceUp)
            {
                if (inColumn.TopmostCard == this)
                {
                    _card.isFaceUp = true;
                    UpdateImage();
                }
                return;
            }
            if (inSuitPile != null && inSuitPile.TopCard != this) return;
            if (Solitaire.Instance.DrawPile.NonDraggableCard(this)) return;
            _draggingCard = this;
            initMousePos = eventData.position;
            initCardPos = transform.position;
            initParent = transform.parent;
            PlaceToParent(DragParent.Transform);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_draggingCard != this) return;

            _draggingCard = null;
            eventData.position = transform.position;
            DropTarget dropTarget = null;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            foreach (var result in results)
            {
                if (result.gameObject != gameObject)
                {
                    dropTarget = result.gameObject.GetComponent<DropTarget>();
                    if (dropTarget != null)
                    {
                        if (dropTarget.DropTo(this))
                        {
                            return;
                        }
                    }
                }
            }
            PlaceToParent(initParent);
            transform.position = initCardPos;
        }

        public void OnPointerMove(PointerEventData eventData)
        {
            if (_draggingCard == this)
            {
                Vector2 offset = eventData.position - initMousePos;
                transform.position = initCardPos + offset;
            }
        }

        public override bool DoesApply(CardUI candidate)
        {
            if (!Card.isFaceUp) return false;
            if (inSuitPile != null)
            {
                return inSuitPile.DoesApply(candidate);
            }
            if (inColumn != null)
            {
                return inColumn.DoesApply(candidate);
            }
            return false;
        }

        public override bool DropTo(CardUI card)
        {
            if (!DoesApply(card))
            {
                return false;
            }
            if (inSuitPile != null)
            {
                return inSuitPile.DropTo(card);
            }
            if (inColumn != null)
            {
                return inColumn.DropTo(card);
            }
            return false;
        }
    }
}
