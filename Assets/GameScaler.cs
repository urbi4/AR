using UnityEngine;

public class GameScaler : MonoBehaviour
{
    public MyGameManager myGameManager;
    private int screenHeight;
    public void Scale()
    {
        screenHeight = Screen.height;
        myGameManager.ScanSign.GetComponent<RectTransform>().localPosition = new Vector3(0, 320 + screenHeight/10, 0);
        myGameManager.ScanBack.GetComponent<RectTransform>().localPosition = new Vector3(0, -320 - screenHeight / 10, 0);
        myGameManager.ArNavigationPanel.GetComponent<RectTransform>().localPosition = new Vector3(0, 270 + screenHeight / 10, 0);
        myGameManager.ArBack.GetComponent<RectTransform>().localPosition = new Vector3(0, -320 - screenHeight / 10, 0);
    }

}
