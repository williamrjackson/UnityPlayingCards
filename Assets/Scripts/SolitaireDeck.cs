using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

namespace Wrj
{
    public class SolitaireDeck : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private Image deckUiImage;
        [SerializeField]
        private Sprite emptyDeckSprite;
        [SerializeField]
        private SolitaireDrawPile drawPile;

        public void Start()
        {
            Solitaire.Instance.OnBegin += () => SetSprite();
            Solitaire.Instance.OnReset += (() =>
            {
                deckUiImage.sprite = emptyDeckSprite;
            });
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            Solitaire.Instance.DrawUpToThree();
            SetSprite();
        }
        public void SetSprite()
        {
            deckUiImage.sprite = (Solitaire.Instance.Deck.DealerCount > 0) ? Solitaire.Instance.BackSprite : emptyDeckSprite;
        }
    }
}
