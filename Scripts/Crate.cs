using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers;

    private SpriteRenderer spriteRenderer;


    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool GettingPushed(Vector3 moveDir)
    {
        gameObject.layer = 0;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, moveDir, 1f, targetLayers);
        gameObject.layer = 6;
        
        if(hit.collider == null)
        {
            transform.localPosition += moveDir;
            
            return true;
        }
        return false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Goal")
        {
            spriteRenderer.color = Color.green;
            
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Goal")
        {
            spriteRenderer.color = Color.green;
            GameManager.Instance.SetWinCount(1);
            GameManager.Instance.WinCheck();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Goal")
        {
            spriteRenderer.color = Color.white;
            GameManager.Instance.SetWinCount(-1);
            GameManager.Instance.WinCheck();
        }
    }

}
