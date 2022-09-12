using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wrj
{
    public class DragParent : MonoBehaviour
    {
        private static DragParent _instance;
        public static DragParent Instance
        {
            get => _instance;
        }
        public static Transform Transform => Instance.gameObject.transform;
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
    }
}
