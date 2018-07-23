using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// test unity input
/// </summary>
public class InputTest : MonoBehaviour {
    public Text box;

    public bool fire1 = false;
    public bool fire2 = false;
    public bool fire3 = false;
    public bool jump = false;
    public bool submit = false;
    public bool cancel = false;

    public float vertical = 0;
    public float horizontal = 0;

    private void Awake()
    {
        Debug.Assert(box != null);
    }

    void Update () {
        fire1 = Input.GetButton("Fire1");
        fire2 = Input.GetButton("Fire2");
        fire3 = Input.GetButton("Fire3");
        jump = Input.GetButton("Jump");
        submit = Input.GetButton("Submit");
        cancel = Input.GetButton("Cancel");

        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");

        box.text = CreateMessage();
	}

    string CreateMessage()
    {
        var lines = new string[]
        {
            $"Fire1\t= {fire1}",
            $"Fire2\t= {fire2}",
            $"Fire3\t= {fire3}",
            $"Jump\t= {jump}",
            $"Submit\t= {submit}",
            $"Cancel\t= {cancel}",
            $"",
            $"Vertical\t= {vertical}",
            $"Horizontal\t= {horizontal}",
        };
        return string.Join("\n", lines);
    }
}
