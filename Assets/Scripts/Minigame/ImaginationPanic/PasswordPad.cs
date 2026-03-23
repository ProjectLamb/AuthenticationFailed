using UnityEngine;
using Photon.Pun;
using TMPro; // TextMeshPro 사용을 위해 필수!

public class PasswordPad : MonoBehaviourPun
{
    [Header("발판 설정")]
    [Tooltip("이 발판이 의미하는 숫자 (0~9)")]
    public int padNumber = 0;

    [Header("시각 효과 연결")]
    public Material redMat;
    public Material greenMat;
    public TextMeshPro floatingText; // 3D TextMeshPro 연결

    private Renderer meshRenderer;
    private bool isActivated = false; // 이미 밟아서 초록색이 된 상태인지

    void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        meshRenderer.material = redMat; // 초기 빨간색

        // [핵심] 게임이 시작되면 인스펙터에 적은 padNumber를 텍스트에 자동으로 박아줍니다.
        if (floatingText != null)
        {
            floatingText.text = padNumber.ToString();

            // 텍스트를 두껍게(Bold) 만들고, 가독성을 위해 흰색으로 세팅
            floatingText.fontStyle = FontStyles.Bold;
            floatingText.color = Color.white;
            floatingText.alignment = TextAlignmentOptions.Center;
        }
    }

    // P1(Player 태그)이 발판에 닿았을 때 (트리거 충돌)
    private void OnTriggerEnter(Collider other)
    {
        // 내가 로컬 플레이어(1P 본인)이고, 아직 안 밟은 발판일 때만 체크
        if (other.CompareTag("Player") && !isActivated)
        {
            PhotonView targetView = other.GetComponent<PhotonView>();

            // PhotonView의 IsMine 체크로 내 캐릭터가 밟은 건지 확인
            if (targetView != null && targetView.IsMine)
            {
                // 정상적으로 밟혔는지 확인하기 위한 디버그 로그
                Debug.Log($"[디버그] 1P가 {padNumber}번 발판을 밟았습니다! 매니저에게 전송합니다.");

                // 매니저에게 내가 밟은 발판의 숫자와 ViewID를 넘겨서 정답인지 채점해달라고 요청
                ImaginationPanic_RpcManager.Instance.OnPadStepped(padNumber, photonView.ViewID);
            }
        }
    }

    // 매니저(서버)에서 채점 후, 정답이면 이 발판을 초록색으로 만듦
    [PunRPC]
    public void RpcUpdatePadVisual(int targetViewID, bool isCorrect)
    {
        if (photonView.ViewID == targetViewID && isCorrect)
        {
            meshRenderer.material = greenMat;
            isActivated = true;
        }
    }

    public void ResetPadLocal()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material = redMat;
        }
        isActivated = false;
    }
}