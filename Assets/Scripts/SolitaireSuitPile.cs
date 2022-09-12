using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wrj
{
    public class SolitaireSuitPile : DropTarget
    {
        private PlayingCards.Suit _establishedSuit = PlayingCards.Suit.None;
        public PlayingCards.Suit EstablishedSuit { get => _establishedSuit; }
        public string LocationId => GetInstanceID().ToString();
        private List<CardUI> inPile = new List<CardUI>();
        private Coroutine rebuildRoutine = null;
        public CardUI TopCard
        {
            get
            {
                var cards = Solitaire.Instance.Deck.GetCardsByLocationId(GetInstanceID().ToString());
                if (cards != null && cards.Length > 0)
                {
                    return CardUI.GetCardUI(cards[0]);
                }
                return null;
            }
        }

        private void Start()
        {
            PlayingCards.onCardLocationChanged += HandleMovedCard;
            Solitaire.Instance.OnReset += Reset;
        }

        private void Reset()
        {
            Wipe();
            inPile = new List<CardUI>();
            _establishedSuit = PlayingCards.Suit.None;
        }

        private void HandleMovedCard(PlayingCards.Card cardData)
        {
            if (cardData.LocationId == LocationId)
            {
                if (rebuildRoutine == null)
                {
                    rebuildRoutine = StartCoroutine(RebuildRoutine());
                }
            }
        }

        private IEnumerator RebuildRoutine()
        {
            yield return new WaitForEndOfFrame();
            Wipe();
            var cards = Solitaire.Instance.Deck.GetCardsByLocationId(LocationId);
            for (int i = cards.Length - 1; i >= 0; i--)
            {
                Stack(cards[i]);
            }
            Solitaire.Instance.CheckWinState();
            rebuildRoutine = null;
        }
        private void Wipe()
        {
            foreach (var item in inPile)
            {
                item.Park();
            }
            inPile.Clear();
        }
        private void Stack(PlayingCards.Card cardData)
        {
            var card = CardUI.GetCardUI(cardData);
            card.PlaceToParent(transform);
            card.transform.localPosition = Vector3.zero;
            inPile.Add(card);
        }

        public override bool DoesApply(CardUI card)
        {
            if (_establishedSuit == PlayingCards.Suit.None)
            {
                if (card.Card.Value == PlayingCards.Value.Ace)
                {
                    return true;
                }
            }
            else if (card.Card.Suit != _establishedSuit)
            {
                return false;
            }
            else if (card.Card.ValueIndex == TopCard.Card.ValueIndex + 1)
            {
                return true;
            }
            return false;
        }

        public override bool DropTo(CardUI card)
        {
            if (!DoesApply(card))
            {
                return false;
            }
            card.inSuitPile = this;
            card.inColumn = null;
            _establishedSuit = card.Card.Suit;
            card.Card.LocationId = LocationId;
            return true;
        }
    }
}
