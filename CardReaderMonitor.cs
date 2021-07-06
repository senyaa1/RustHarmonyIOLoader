using UnityEngine;
using Harmony;

namespace HarmonyIOLoader
{
    class CardReaderMonitor : MonoBehaviour
    {
        private CardReader cardReader;
        private float accessDuration;
        private bool isEnabled;
        private void Awake()
        {   
            cardReader = GetComponent<CardReader>();
            enabled = false;
        }
        private void Update()
        {
            if (!isEnabled && cardReader.HasFlag((BaseEntity.Flags)2))
            {
                isEnabled = true;
                cardReader?.Invoke(new System.Action(Reset), accessDuration);
            }
        }
        private void Reset()
        {
            cardReader?.ResetIOState();
            isEnabled = false;
        }

        public void Init(float duration)
        {
            accessDuration = duration;
            if (duration <= 0f)
                Destroy(this);
            else
            {
                Reset();
                enabled = true;
           } 
        }
    }
}
