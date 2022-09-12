using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wrj
{
    public class SolitaireColumnManager : MonoBehaviour
    {
        [SerializeField]
        SolitaireColumn[] columns;

        public int ColumnCount => columns.Length;
        public SolitaireColumn GetColumnByIndex(int index)
        {
            if (index < columns.Length && index >= 0)
            {
                return columns[index];
            }
            return null;
        }
    }
}
