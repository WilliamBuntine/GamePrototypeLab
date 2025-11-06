using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerIntroduction : MonoBehaviour
{
    public PlayerMove player;
    public GrappleBoost playerGrapple;
    public Swinging playerSwing;
    public Camera playerCam;
    public Transform lookPoint1;
    public Transform lookPoint2;
    private Transform targetLookPoint;


    public float jumpWait = 0.9f;
    public float grappleWait = 0.7f;
    public float grappleReleaseWait = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LookatPoint(lookPoint1);
        StartCoroutine(WaitForJump());
    }

    // Update is called once per frame
    void Update()
    {
        if (targetLookPoint != null)
        {
            playerCam.transform.LookAt(targetLookPoint);
        }
    }


    void LookatPoint(Transform lookPoint)
    {
        player.walkingEnabled = false;
        player.sprintingEnabled = false;
        player.jumpingEnabled = true;
        player.slidingEnabled = false;
        player.mouseMoveEnabled = false;
        playerSwing.mouseLeftEnabled = false;
        playerGrapple.mouseRightEnabled = false;
        targetLookPoint = lookPoint;
        Vector3 direction = lookPoint.position - player.transform.position;

        direction.y = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        player.transform.rotation = targetRotation;
    }

    IEnumerator WaitForJump()
    {
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

        yield return new WaitForSeconds(jumpWait);

        player.FreezePlayer();

        StartCoroutine(WaitForGrapple());

    }

    IEnumerator WaitForGrapple()
    {

        //Print message here telling player to hit Right mouse button

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Mouse1));

        player.UnfreezePlayer();

        playerGrapple.TryStartGrapple();

        playerGrapple.iHasControl = false;

        targetLookPoint = null;
        
        yield return new WaitForSeconds(grappleWait);

        targetLookPoint = lookPoint2;


        player.FreezePlayer();

        StartCoroutine(WaitForGrappleRelease());
    }

    IEnumerator WaitForGrappleRelease()
    {
        playerGrapple.iHasControl = true;

        //Print message here telling player to release Right mouse button

        yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Mouse1));

        player.UnfreezePlayer();

        playerGrapple.StopGrapple();

        targetLookPoint = lookPoint2;
        LookatPoint(lookPoint2);

        
        yield return new WaitForSeconds(grappleReleaseWait);

        player.FreezePlayer();

        StartCoroutine(WaitForSwing());

    }

    IEnumerator WaitForSwing()
    {
        //Print message here telling player to hit Left mouse button

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Mouse0));

        player.UnfreezePlayer();

        playerSwing.StartSwing();

        targetLookPoint = null;
        
        yield return new WaitForSeconds(grappleReleaseWait);

        //Print message here explaining slide and wall jump, release mouse to finish!

        yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Mouse0));


        player.walkingEnabled = true;
        player.sprintingEnabled = true;
        player.jumpingEnabled = true;
        player.slidingEnabled = true;
        player.mouseMoveEnabled = true;
        playerSwing.mouseLeftEnabled = true;
        playerGrapple.mouseRightEnabled = true;
    }
}
