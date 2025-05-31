using UnityEngine;
using GameJolt.API;
using UnityEngine.SceneManagement;
using GameJolt.UI;

public class LoginSceneManager : MonoBehaviour
{
    void Start()
    {
        GameJoltUI.Instance.ShowSignIn((success) =>
        {
            if (success)
            {
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                Debug.LogError("Login error!");
            }
        });
    }
}
