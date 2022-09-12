using UnityEngine;
using UnityEngine.UI;

namespace Wrj
{
    public class RandomDeal : MonoBehaviour
    {
        [SerializeField]
        TMPro.TextMeshProUGUI drawnCardLabel;
        [SerializeField]
        Image drawnCardRenderer;
        [SerializeField]
        private PlayingCards playingCards;
        private PlayingCards.CardDeck cards;

        private void ShuffleNewDeck()
        {
            cards = new PlayingCards.CardDeck(false, playingCards.SpriteFactory);
            cards.Shuffle();
        }
        PlayingCards.Card drawnCard = null;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                if (cards == null || cards.DealerCards.Length == 0)
                {
                    ShuffleNewDeck();
                }
                if (drawnCard == null || drawnCard.isFaceUp)
                {
                    if (drawnCard != null)
                    {
                        drawnCard.LocationId = "Used";
                    }
                    drawnCard = cards.Deal("OnScreen");
                    drawnCardLabel.SetText("");
                }
                else
                {
                    drawnCard.isFaceUp = true;
                    drawnCardLabel.SetText(drawnCard.ToString());
                }
                drawnCardRenderer.sprite = drawnCard.Sprite;
            }
        }

    }
}
