using UnityEngine;

public class Wall : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("DC_PaperPlane"))
        {
            Destroy(other.gameObject);
        }
    }
}