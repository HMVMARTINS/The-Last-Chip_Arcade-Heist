using UnityEngine;
using UnityEngine.Events;

public class TriggersManager : MonoBehaviour
{
    [SerializeField]
    private UnityEvent[] actions;

    [SerializeField]
    private Collider[] colliders;

    [SerializeField]
    bool[] destroyAfterUse;

    private void Awake()
    {
        if (actions.Length != colliders.Length)
        {
            Debug.LogError("Collider number doesn't match action number");
            return;
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider col = colliders[i];
            col.isTrigger = true;

            TriggerHelper helper = col.gameObject.GetComponent<TriggerHelper>();
            if (helper == null)
                helper = col.gameObject.AddComponent<TriggerHelper>();

            helper.SetAction(actions[i], destroyAfterUse[i]);
        }
    }

    private class TriggerHelper : MonoBehaviour
    {
        private UnityEvent onTriggered;
        private bool destroyAfterUse;

        public void SetAction(UnityEvent action, bool destroyAfterUse)
        {
            onTriggered = action ?? new UnityEvent();
            this.destroyAfterUse = destroyAfterUse;
        }

        private void OnTriggerEnter(Collider other)
        {
            onTriggered?.Invoke();
            if (destroyAfterUse)
                gameObject.SetActive(false);
        }
    }
}
