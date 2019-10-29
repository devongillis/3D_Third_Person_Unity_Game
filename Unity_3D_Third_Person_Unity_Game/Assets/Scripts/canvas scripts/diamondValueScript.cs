using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class diamondValueScript : MonoBehaviour
{
    // this script should animate the diamond
    // and also accept a call to increment the text
    // Start is called before the first frame update
    
    public UnityEngine.UI.Text textBox;


    void Start()
    {
        textBox = gameObject.GetComponent<UnityEngine.UI.Text>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateValue(int value)
    {
        textBox.text = "x" + value.ToString();
    }
}
