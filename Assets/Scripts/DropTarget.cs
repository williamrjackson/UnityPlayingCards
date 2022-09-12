using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wrj
{
    public abstract class DropTarget : MonoBehaviour
    {
        public abstract bool DoesApply(CardUI card);
        public abstract bool DropTo(CardUI card);
    }
}
