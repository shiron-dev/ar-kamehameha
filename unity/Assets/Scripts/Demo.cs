using UnityEngine;
using System.Collections;

public class Demo : MonoBehaviour
{
    [SerializeField]
	private MainController mainController;

    [SerializeField]
    private GameObject view;
    [SerializeField]
    private GameObject demo;

	// Use this for init
    // ialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
        if (mainController.noMove)
        {
            view.SetActive(false);
            demo.SetActive(true);
        }
        else
        {
            view.SetActive(true);
            demo.SetActive(false);
        }
	}
}

