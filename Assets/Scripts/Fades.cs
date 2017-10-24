using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Fades : MonoBehaviour {

    public static Fades current;

    public bool finished;

    public float FadeDuration;

    private Image panel;

	// Use this for initialization
	void Start () {
        current = this;
        panel = GetComponent<Image>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public IEnumerator FadeOut()
    {
        finished = false;

        panel.raycastTarget = true;

        float timeElapsed = 0;

        float percentage = timeElapsed / FadeDuration;

        while(percentage < 1)
        {
            timeElapsed += Time.deltaTime;
            percentage = timeElapsed / FadeDuration;

            panel.color = new Color(1, 1, 1, percentage);

            yield return null;
        }

        finished = true;
    }

    public IEnumerator FadeIn()
    {
        finished = false;

        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            Vector3 difference = CameraBehaviour.current.transform.position - CameraBehaviour.current.targetPosition;
            while (difference.magnitude > 0.1f)
            {
                difference = CameraBehaviour.current.transform.position - CameraBehaviour.current.targetPosition;
                yield return null;
            }
        }

        float timeElapsed = 0;

        float percentage = timeElapsed / FadeDuration;

        while (percentage < 1)
        {
            timeElapsed += Time.deltaTime;
            percentage = timeElapsed / FadeDuration;

            panel.color = new Color(1, 1, 1, 1-percentage);

            yield return null;
        }

        panel.raycastTarget = false;

        finished = true;
    }
}
