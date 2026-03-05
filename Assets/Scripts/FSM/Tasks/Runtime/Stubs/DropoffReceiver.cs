using UnityEngine;

public class DropoffReceiver : MonoBehaviour
{
    private void OnEnable()
    {
        DropoffLocator.Register(this);
    }

    private void OnDisable()
    {
        DropoffLocator.Unregister(this);
    }

    public void Receive(int amount)
    {
        Debug.Log($"{name} received {amount} items.");
    }
}
