using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils
{
    public class MySceneLoader : MonoBehaviour
    {
        public static void LoadLevel(int num)
        {
            if (num > 0 && num <= 5)
            {
                LoadFloor(num);
            }
            else
            {
                LoadTutorial();
            }
        }

        public static void LoadMainMenu()
        {
            SceneManager.LoadScene("StartMenu");
        }

        public static void LoadSettings()
        {
            SceneManager.LoadScene("SettingsMenu");
        }

        #region Levels

        public static void LoadTutorial()
        {
            SceneManager.LoadScene("Tutorial");
        }

        public static void LoadFloor(int idx)
        {
            SceneManager.LoadScene("Floor " + idx);
        }

        public static void LoadArcade()
        {
            SceneManager.LoadScene("Arcade");
        }
        public static void LoadLeaderBoards()
        {
            SceneManager.LoadScene("Leader Boards");
        }
        public static void LoadArcadeEndScreen()
        {
            SceneManager.LoadScene("ArcadeEndScreen");
        }

        #endregion
    }
}