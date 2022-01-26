using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinuousGameSaver : MonoBehaviour
{
    private Transform playerTransform;
    private const float SavingInterval = 1; // seconds
    // Start is called before the first frame update

    void Start()
    {
        playerTransform = SpringController.Instance.GetFaceSegment().transform;
        StartCoroutine(nameof(SaveProgress));
    }

    private IEnumerator SaveProgress()
    {
        while (true)
        {
            PlayerPrefs.SetString("level", SceneManager.GetActiveScene().name);
            var currentPosition = playerTransform.position;
            PlayerPrefs.SetFloat("position_x", currentPosition.x);
            PlayerPrefs.SetFloat("position_y", currentPosition.y);
            PlayerPrefs.Save();
            yield return new WaitForSeconds(SavingInterval);
        }
    }
}
