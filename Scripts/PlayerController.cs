using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField]private LayerMask targetLayers;
    [SerializeField] private Button leftBtn;
    [SerializeField] private Button rightBtn;
    [SerializeField] private Button downBtn;
    [SerializeField] private Button upBtn;


    private Rigidbody2D _playerRb;

    // Start is called before the first frame update
    void Start()
    {
        _playerRb = GetComponent<Rigidbody2D>();
        leftBtn.onClick.AddListener(() => MoveOrPush(Vector3.left));
        rightBtn.onClick.AddListener(() => MoveOrPush(Vector3.right));
        downBtn.onClick.AddListener(() => MoveOrPush(Vector3.down));
        upBtn.onClick.AddListener(() => MoveOrPush(Vector3.up));

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.isGameOver) return;
        
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            MoveOrPush(Vector3.right);
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            MoveOrPush(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveOrPush(Vector3.up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveOrPush(Vector3.down);
        }


    }


    private void MoveOrPush(Vector3 moveDir)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, 1f, targetLayers);
        bool isWallHit = false;
        bool isCrateHit = false;

        if (hit.collider != null)
        {
            
            switch (hit.collider.gameObject.layer)
            {
                case 6:
                    isCrateHit = true;
                    break;
                case 7:
                    isWallHit = true;
                    break;
            }
        }
        if (isWallHit)
        {
            return;
        }
        if (isCrateHit)
        {
            Crate crate = hit.collider.GetComponent<Crate>();
            bool isCratePushed = crate.GettingPushed(moveDir);
            if (!isCratePushed) return;
        }

        transform.localPosition += moveDir;
        

    }
}
