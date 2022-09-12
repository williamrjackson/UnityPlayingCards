using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wrj
{
    public class SolitaireDrawPile : MonoBehaviour
    {
        [SerializeField]
        Transform topThreeParent;
        [SerializeField]
        Transform remainderParent;

        public string LocationId => GetInstanceID().ToString();

        private Coroutine rebuildRoutine;
        private List<PlayingCards.Card> _drawPileCards = new List<PlayingCards.Card>();

        private void Start()
        {
            PlayingCards.onCardLocationChanged += HandleCardMove;
        }

        private void HandleCardMove(PlayingCards.Card card)
        {
            // If the card moved from here, removed it from the list
            if (card.LastLocationId == LocationId && _drawPileCards.Contains(card))
            {
                _drawPileCards.Remove(card);
                if (_drawPileCards.Count == 0 && rebuildRoutine == null)
                {
                    rebuildRoutine = StartCoroutine(RebuildDrawPileRoutine());
                }
            }
            // If the card was added here, schedule a rebuild of the pile
            // for the ext frame
            if (card.LocationId == LocationId)
            {
                if (rebuildRoutine == null)
                {
                    rebuildRoutine = StartCoroutine(RebuildDrawPileRoutine());
                }
            }
        }

        public bool NonDraggableCard(CardUI cardUi)
        {
            var card = cardUi.Card;
            if (!card.isFaceUp) return true;
            if (!_drawPileCards.Contains(card)) return false;
            if (_drawPileCards.IndexOf(card) == 0) return false;
            return true;
        }

        private IEnumerator RebuildDrawPileRoutine()
        {
            yield return new WaitForEndOfFrame();
            WipeChildren();
            _drawPileCards = new List<PlayingCards.Card>(Solitaire.Instance.Deck.GetCardsByLocationId(LocationId));
            for (int i = _drawPileCards.Count - 1; i >= 0; i--)
            {
                CardUI newCard = CardUI.GetCardUI(_drawPileCards[i]);
                if (i < 3)
                {
                    newCard.PlaceToParent(topThreeParent);
                }
                else
                {
                    newCard.PlaceToParent(remainderParent);
                    newCard.transform.localPosition = Vector3.zero;
                }
            }
            rebuildRoutine = null;
        }

        private void WipeChildren()
        {
            var childCards = transform.GetComponentsInChildren<CardUI>();
            foreach (var item in childCards)
            {
                item.Park();
            }
            childCards = topThreeParent.GetComponentsInChildren<CardUI>();
            foreach (var item in childCards)
            {
                item.Park();
            }
            _drawPileCards.Clear();
        }
    }
}
