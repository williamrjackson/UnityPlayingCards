using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Wrj
{
    public class Solitaire : MonoBehaviour
    {
        [SerializeField]
        private PlayingCards playingCards;
        [SerializeField]
        private SolitaireColumnManager columnManager;
        [SerializeField]
        private SolitaireDrawPile drawPile;
        [SerializeField]
        private SolitaireSuitPile[] suitPiles;
        [SerializeField]
        [Range(0f, .3f)]
        private float dealSpeed = .1f;
        [SerializeField]
        private CardUI cardPrefab;
        [SerializeField]
        private Transform parkingLot;
        [SerializeField]
        private Transform winBanner;

        private PlayingCards.CardDeck _deck;
        private Coroutine _dealRoutine = null;

        public PlayingCards.CardDeck Deck { get => _deck; }
        public SolitaireDrawPile DrawPile { get => drawPile; }
        public Sprite BackSprite => playingCards.SpriteFactory.BackSprite;
        public UnityAction OnReset;
        public UnityAction OnBegin;
        public static Transform ParkingLot { get { return _instance.parkingLot; } }
        public static Solitaire Instance => _instance;
        private static Solitaire _instance;
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public void ResetGame()
        {
            winBanner.gameObject.SetActive(false);
            CardUI.DestroyAllCards();
            if (OnReset != null)
            {
                OnReset();
            }
        }
        public void Deal()
        {
            if (_dealRoutine != null)
            {
                StopCoroutine(_dealRoutine);
            }
            ResetGame();
            if (_dealRoutine != null)
            {
                StopCoroutine(_dealRoutine);
            }
            _deck = new PlayingCards.CardDeck(false, playingCards.SpriteFactory);
            _deck.AcesHigh = false;

            foreach (var item in _deck.DealerCards)
            {
                var newCard = Instantiate(cardPrefab);
                newCard.Card = item;
                newCard.Park();
            }

            _deck.Shuffle();
            _dealRoutine = StartCoroutine(DealCardsRoutine());
        }
        private IEnumerator DealCardsRoutine()
        {
            int startIndex = 0;
            yield return new WaitForSeconds(dealSpeed);
            while (startIndex < columnManager.ColumnCount)
            {
                var column = columnManager.GetColumnByIndex(startIndex++);
                string columnId = column.GetInstanceID().ToString();
                column.DealIn(_deck.Draw(columnId));
                int increment = startIndex;
                yield return new WaitForSeconds(dealSpeed);
                while (increment < columnManager.ColumnCount)
                {
                    column = columnManager.GetColumnByIndex(increment++);
                    columnId = column.GetInstanceID().ToString();
                    column.DealIn(_deck.Deal(columnId));
                    yield return new WaitForSeconds(dealSpeed);
                }
            }
            if (OnBegin != null)
            {
                OnBegin();
            }
        }

        public bool CheckWinState()
        {
            foreach (var item in suitPiles)
            {
                if (item.TopCard.Card.Value != PlayingCards.Value.King)
                {
                    return false;
                }
            }
            winBanner.gameObject.SetActive(true);
            return true;
        }

        public void DrawUpToThree()
        {
            if (_deck == null)
            {
                Deal();
                return;
            }
            int numToDraw = System.Math.Min(3, _deck.DealerCount);
            var drawPileCards = _deck.GetCardsByLocationId(drawPile.LocationId);
            if (numToDraw == 0 && drawPileCards.Length > 0)
            {
                PurgeDrawPile();
                return;
            }
            for (int i = 0; i < numToDraw; i++)
            {
                _deck.Draw(drawPile.LocationId);
            }
        }

        private void PurgeDrawPile()
        {
            var drawPileCards = _deck.GetCardsByLocationId(drawPile.LocationId);
            foreach (var card in drawPileCards)
            {
                card.LocationId = _deck.DealerId;
            }
        }
    }
}
