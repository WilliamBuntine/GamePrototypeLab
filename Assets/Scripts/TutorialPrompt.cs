using UnityEngine;

public class TutorialPrompt : MonoBehaviour
{
    public PlayerIntroduction playerIntro;
    public PlayerMove player;
    public Swinging playerSwing;
    public GrappleBoost playerGrapple;
    public Grayscale grayscale;


    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        grayscale.EnableGrayscale(true);
        player.FreezePlayer();
        player.mouseMoveEnabled = false;
        playerSwing.mouseLeftEnabled = false;
        playerGrapple.mouseRightEnabled = false;
    }

    public void StartTutorial()
    {
        playerIntro.BeginIntro();
        Cursor.lockState = CursorLockMode.Locked;
        Destroy(gameObject);
    }


    public void DeleteTutorialSequence()
    {
        Destroy(playerIntro.gameObject);
        Cursor.lockState = CursorLockMode.Locked;
        grayscale.EnableGrayscale(false);
        player.mouseMoveEnabled = true;
        player.UnfreezePlayer();
        player.mouseMoveEnabled = true;
        playerSwing.mouseLeftEnabled = true;
        playerGrapple.mouseRightEnabled = true;
        Destroy(gameObject);
    }
}
