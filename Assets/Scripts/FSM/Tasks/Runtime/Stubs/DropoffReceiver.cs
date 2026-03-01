using UnityEngine;

public class DropoffReceiver : MonoBehaviour
{
    public void Receive(int amount)
    {
        Debug.Log($"{name} received {amount} items.");
    }
}