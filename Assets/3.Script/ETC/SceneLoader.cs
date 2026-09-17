using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string scene_name;
    [SerializeField] private Text loadtext;

    private IEnumerator LoadScene_co(string index)
    {
        AsyncOperation load_op = SceneManager.LoadSceneAsync(index);
        load_op.allowSceneActivation = false;

        float timer = 0f;
        float percentage = 0f;

        while (!load_op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;
            if (percentage >= 90)
            {
                percentage = Mathf.Lerp(percentage, 100, timer);
                if (percentage.Equals(100f))
                {
                    load_op.allowSceneActivation = true;
                }
            }
            else
            {
                percentage = Mathf.Lerp(percentage, load_op.progress * 100f, timer);
                if (percentage >= 90)
                {
                    timer = 0;
                }
                loadtext.text = percentage.ToString("0") + "%";
            }
        }
    }
}
