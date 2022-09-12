using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wrj
{
    public class SolitaireColumn : DropTarget
    {
        private List<CardUI> cardUIs = new List<CardUI>();
        public string LocationId => GetInstanceID().ToString();
        private Coroutine rebuildRoutine = null;
        private void Start()
        {
            Solitaire.Instance.OnReset += () => { cardUIs.Clear(); };
            PlayingCards.onCardLocationChanged += HandleMovedCard;
        }

        private void HandleMovedCard(PlayingCards.Card card)
        {
            if (card.LocationId == LocationId || card.LastLocationId == LocationId)
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
            WipeColumn();
            var cards = Solitaire.Instance.Deck.GetCardsByLocationId(LocationId);
            for (int i = cards.Length - 1; i >= 0 ; i--)
            {
                DealIn(cards[i]);
            }
            rebuildRoutine = null;
        }

        private void WipeColumn()
        {
            foreach (var item in cardUIs)
            {
                if (item.inColumn == this)
                {
                    item.inColumn = null;
                    item.Park();
                }
            }
            cardUIs.Clear();
        }

        public void DealIn(PlayingCards.Card card)
        {
            Transform targetParent = transform;
            float relativeY = 0f;
            bool shouldAddChild = false;
            if (TopmostCard != null)
            {
                shouldAddChild = true;
                targetParent = TopmostCard.transform;
                relativeY = (TopmostCard.Card.isFaceUp) ? -30f : -10f;
            }
            CardUI newCard = CardUI.GetCardUI(card);
            newCard.PlaceToParent(targetParent);
            newCard.transform.localPosition = Vector3.zero.With(y: relativeY);
            newCard.inColumn = this;
            if (shouldAddChild) TopmostCard.childCard = newCard;
            cardUIs.Add(newCard);
        }

        public override bool DoesApply(CardUI candidate)
        {
            if (TopmostCard == null)
            {
                return (candidate.Card.Value == PlayingCards.Value.King);
            }
            if (TopmostCard.Card.SuitColor != candidate.Card.SuitColor &&
                TopmostCard.Card.ValueIndex == candidate.Card.ValueIndex + 1)
            {
                return true;
            }
            return false;
        }

        public override bool DropTo(CardUI card)
        {
            if (!DoesApply(card)) return false;

            card.Card.LocationId = LocationId;
            CardUI child = card.childCard;
            while (child != null)
            {
                child.Card.SetLocationIdWithoutNotify(LocationId);
                child = child.childCard;
            }
            return true;
        }

        public CardUI TopmostCard
        {
            get
            {
                if (cardUIs != null && cardUIs.Count > 0)
                {
                    return cardUIs[cardUIs.Count - 1];
                }
                return null;
            }
        }
    }
}
