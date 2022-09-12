using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wrj
{
    public class PlayingCards : MonoBehaviour
    {
        [SerializeField]
        private Sprite[] cardSheet;

        private static PlayingCards _instance;
        public static PlayingCards Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("No PlayingCards instance in scene.");
                    return null;
                }
                return _instance;
            }
        }
        private CardSpriteFactory _spriteFactory;
        public CardSpriteFactory SpriteFactory
        {
            get
            {
                if (_spriteFactory == null)
                {
                    InitSpriteFactory();
                }
                return _spriteFactory;
            }
        }

        public delegate void OnCardLocationChanged(Card card);
        public static OnCardLocationChanged onCardLocationChanged;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Debug.LogWarning($"Multiple PlayingCard instances found in scene.\n" +
                    $"Removed from {gameObject.name}", this);
            }
        }

        private void InitSpriteFactory()
        {
            _spriteFactory = new CardSpriteFactory(cardSheet, Suit.Clubs, Suit.Diamonds, Suit.Hearts, Suit.Spades, 54, 52);
        }

        public enum Suit { None, Clubs, Diamonds, Hearts, Spades }
        public enum Value { None, Ace, Deuce, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King }
        public enum SuitColor { Red, Black }
        public class Card
        {
            private Suit _suit;
            public Suit Suit { get => _suit; }
            private Value _value;
            public Value Value { get => _value; }
            private CardDeck _rootDeck = null;
            private Sprite _sprite;
            private Sprite _backSprite;
            public Sprite Sprite
            {
                get
                {
                    return (isFaceUp) ? _sprite : _backSprite;
                }
            }
            private string _lastLocationId = string.Empty;
            public string LastLocationId { get => _lastLocationId; }
            private string _locationId = string.Empty;
            public string LocationId
            {
                set
                {
                    _lastLocationId = _locationId;
                    _locationId = value;
                    if (_rootDeck != null)
                    {
                        _positionInLocation = _rootDeck.GetCardsByLocationId(_locationId).Length - 1;
                    }
                    if (PlayingCards.onCardLocationChanged != null)
                    {
                        PlayingCards.onCardLocationChanged(this);
                    }
                }
                get => _locationId;
            }
            public void SetLocationIdWithoutNotify(string locationId)
            {
                _lastLocationId = _locationId;
                _locationId = locationId;
                if (_rootDeck != null)
                {
                    _positionInLocation = _rootDeck.GetCardsByLocationId(_locationId).Length - 1;
                }
            }
            private int _positionInLocation = 0;
            public int PositionInLocation { get => _positionInLocation; }
            public bool aceHigh = true;
            public SuitColor SuitColor =>(_suit == Suit.Diamonds || _suit == Suit.Hearts) ? SuitColor.Red : SuitColor.Black;
            public Sprite Face => _sprite;
            public Sprite Back => _backSprite;

            public bool isFaceUp = false;
            public bool IsJoker => _suit == Suit.None || _value == Value.None;
            public int ValueIndex {
                get
                {
                    if (aceHigh)
                    {
                        if (_value == Value.Ace) return 14;
                    }
                    int index = Array.IndexOf(Enum.GetValues(typeof(Value)), _value);
                    return index;
                }
            }


            public Card(Suit suit, Value val, Sprite sprite, Sprite backSprite, CardDeck cardDeck)
            {
                _suit = suit;
                _value = val;
                _sprite = sprite;
                _backSprite = backSprite;
                _rootDeck = cardDeck;
            }
            public Card(Suit suit, Value val, CardSpriteFactory spriteFactory, CardDeck cardDeck)
            {
                _suit = suit;
                _value = val;
                _sprite = spriteFactory.GetSprite(_suit, _value);
                _backSprite = spriteFactory.BackSprite;
                _rootDeck = cardDeck;
            }
            public override string ToString()
            {
                if (IsJoker) return "Joker";
                return $"{Enum.GetName(typeof(Value), _value)} of {Enum.GetName(typeof(Suit), _suit)}";
            }
        }

        public class CardDeck
        {
            public int DealerCount => _dealer.Count;
            public int TotalCount => _allCards.Count;
            private string _dealerId = String.Empty;
            public string DealerId
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(_dealerId))
                    {
                        _dealerId = Guid.NewGuid().ToString();
                        PlayingCards.onCardLocationChanged += HandleCardLocationChange;
                    }
                    return _dealerId;
                }
            }

            private void HandleCardLocationChange(Card card)
            {
                if (card.LastLocationId == DealerId)
                {
                    _dealer.Remove(card);
                }
                if (card.LocationId == DealerId)
                {
                    _dealer.AddLast(card);
                }
            }

            private bool _acesHigh = true;
            public bool AcesHigh
            {
                get => _acesHigh;
                set
                {
                    if (_acesHigh != value)
                    {
                        _acesHigh = value;
                        foreach (var deckCard in _dealer)
                        {
                            deckCard.aceHigh = _acesHigh;
                        }
                        foreach (var dealtCard in _allCards)
                        {
                            dealtCard.aceHigh = _acesHigh;
                        }
                    }
                }
            }
            private LinkedList<Card> _dealer = new LinkedList<Card>();
            private List<Card> _allCards = new List<Card>();

            public Card[] DealerCards => _dealer.ToArray();
            public Card[] AllCards => _allCards.ToArray();
            public Card[] GetCardsByLocationId(string locationId)
            {
                return _allCards.Where((cardElement) => cardElement.LocationId == locationId).OrderByDescending(card => card.PositionInLocation).ToArray();
            }
            public CardDeck(bool jokers, CardSpriteFactory spriteFactory)
            {
                AddDeck(spriteFactory, jokers);
            }
            public CardDeck()
            {
                AddDeck(new CardSpriteFactory(), false);
            }
            public CardDeck(int numberOfDecks, bool jokers, CardSpriteFactory spriteFactory)
            {
                for (int i = 0; i < numberOfDecks; i++)
                {
                    AddDeck(spriteFactory, jokers);
                }
            }
            public CardDeck(List<Card> cards)
            {
                foreach (var item in cards)
                {
                    AddCard(item);
                }
            }
            public void AddCard(Card card)
            {
                card.isFaceUp = false;
                card.LocationId = DealerId;
                if (!_dealer.Contains(card))
                    _dealer.AddLast(card);
                if (!_allCards.Contains(card))
                    _allCards.Add(card);
            }

            private void AddDeck(CardSpriteFactory spriteFactory, bool jokers = false)
            {
                if (jokers)
                {
                    AddCard(new Card(Suit.None, Value.None, spriteFactory, this));
                    AddCard(new Card(Suit.None, Value.None, spriteFactory, this));
                }
                foreach (Suit suit in (Suit[])Enum.GetValues(typeof(Suit)))
                {
                    if (suit != Suit.None)
                    {
                        foreach (Value val in (Value[])Enum.GetValues(typeof(Value)))
                        {
                            if (val != Value.None)
                                AddCard( new Card(suit, val, spriteFactory, this) );
                        }
                    }
                }
            }
            public void Shuffle(int multiplier = 1)
            {
                multiplier = Math.Clamp(multiplier, 1, 10);
                string tempLocation = new Guid().ToString();
                var cardList = _dealer.ToList();
                foreach (var temp in cardList)
                {
                    temp.SetLocationIdWithoutNotify(tempLocation);
                }
                for (int i = 0; i < multiplier; i++)
                {
                    for (var j = cardList.Count - 1; j > 1; j--)
                    {
                        var k = UnityEngine.Random.Range(0, j + 1);
                        var value = cardList[k];
                        cardList[k] = cardList[j];
                        cardList[j] = value;
                    }
                }
                _dealer.Clear();
                foreach (var item in cardList)
                {
                    item.SetLocationIdWithoutNotify(DealerId);
                    _dealer.AddLast(item);
                }
            }
                
            public Card Draw(string locationId)
            {
                Card result = null;
                if (_dealer.Count > 0)
                {
                    result = _dealer.Last();
                    _dealer.Remove(result);
                    result.isFaceUp = true;
                    result.LocationId = locationId;
                }
                return result;
            }
            public Card Deal(string locationId)
            {
                Card result = null;
                if (_dealer.Count > 0)
                {
                    result = _dealer.Last();
                    _dealer.Remove(result);
                    result.isFaceUp = false;
                    result.LocationId = locationId;
                }
                return result;
            }
        }
        public class CardSpriteFactory
        {
            private Sprite[] _spriteSheet;
            private Suit[] _order;
            private bool _aceHigh;
            private int _jokerIndex;
            private int _backIndex;
            private bool _isNullFactory;

            public CardSpriteFactory()
            {
                _isNullFactory = true;
            }
            public CardSpriteFactory(Sprite[] spriteSheet, Suit a, Suit b, Suit c, Suit d, int backIndex, int jokerIndex = 0, bool aceHigh = false)
            {
                this.Initialize(spriteSheet, a, b, c, d, backIndex, jokerIndex, aceHigh);
            }
            public void Initialize(Sprite[] spriteSheet, Suit a, Suit b, Suit c, Suit d, int backIndex, int jokerIndex = 0, bool aceHigh = false)
            {
                if (spriteSheet.Length < Math.Max(Math.Max(52, backIndex), jokerIndex))
                {
                    _isNullFactory = true;
                    Debug.LogWarning("Invalid CardFactory SpriteSheet");
                    return;
                }
                _spriteSheet = spriteSheet;
                _order = new Suit[] { a, b, c, d };
                _backIndex = backIndex;
                _jokerIndex = jokerIndex;
                _aceHigh = aceHigh;
                _isNullFactory = false;
            }
            public Sprite GetSprite(Suit suit, Value val)
            {
                if (_isNullFactory) return null;
                if (suit == Suit.None || val == Value.None) return _spriteSheet[_jokerIndex];
                return _spriteSheet[(13 * Array.IndexOf(_order, suit)) + ValIndex(val)];
            }
            private int ValIndex(Value val)
            {
                if (val == Value.Ace && _aceHigh) return 12;
                int index = Array.IndexOf(Enum.GetValues(typeof(Value)), val) - 1;
                return index;
            }
            public Sprite BackSprite => (_isNullFactory) ? null : _spriteSheet[_backIndex];
        }
    }
}
