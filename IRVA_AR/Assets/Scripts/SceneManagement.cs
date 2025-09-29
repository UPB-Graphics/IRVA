using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Lab1()
    {
        SceneManager.LoadScene(1);
    }

    public void Lab2()
    {
        SceneManager.LoadScene(2);
    }

    public void Lab3()
    {
        SceneManager.LoadScene(3);
    }

    public void Lab4_1()
    {
        SceneManager.LoadScene(4);
    }

    public void Lab4_2()
    {
        SceneManager.LoadScene(5);
    }

    public void Bonus1()
    {
        SceneManager.LoadScene(6);
    }

    public void Bonus2()
    {
        SceneManager.LoadScene(7);
    }
}
