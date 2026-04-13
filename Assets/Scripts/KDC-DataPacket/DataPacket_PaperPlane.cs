using UnityEngine;
using Photon.Pun;

public class DataPacket_PaperPlane : MonoBehaviourPun
{
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 force)
    {
        photonView.RPC("RpcLaunch", RpcTarget.All, force.x, force.y);
    }

    [PunRPC]
    void RpcLaunch(float x, float y)
    {
        if (rb != null)
            rb.AddForce(new Vector2(x, y), ForceMode2D.Impulse);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // MasterClient만 판정
        if (!PhotonNetwork.IsMasterClient) return;
        if (DataPacket_RpcManager.instance == null) return;

        int phase = DataPacket_RpcManager.instance.currentPhase;

        if (phase == 1 && other.CompareTag("DC_Player2"))
        {
            DataPacket_RpcManager.instance.AddScore(2);
            PhotonNetwork.Destroy(gameObject); // MasterClient가 직접 Destroy
        }
        else if (phase == 2 && other.CompareTag("DC_Player1"))
        {
            DataPacket_RpcManager.instance.AddScoreP2(2);
            PhotonNetwork.Destroy(gameObject); // MasterClient가 직접 Destroy
        }
    }
}