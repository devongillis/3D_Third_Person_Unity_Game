using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyBindScript : MonoBehaviour
{

    private GameObject currentKey;

    public GameObject keyBoardEnabledToggle;
    public GameObject keyBoardInvertCameraToggle;
    public GameObject controllerInvertCameraToggle;

    public GameObject forwardText, backwardText, leftText, rightText, kJumpText, kEnableText, kAttackText, speedUpText, verticalText, horizontalText, cJumpText, cEnableText, cAttackText, cameraVerticalText, cameraHorizontalText;

    public string[] inputs; // all input axis' in settings
    public bool keyBoardEnabled = true;
    public bool keyBoardInvertCamera = true;
    public bool controllerInvertCamera = true;
    public bool inputBoardActive = false;



    public bool calibrateInput = false;
    public bool calibrateCamera = false;
    public float f = 0.01f, b = 0.01f, l = 0.01f, r = 0.01f; // the calibration values
    public float cf = 0.01f, cb = 0.01f, cl = 0.01f, cr = 0.01f; // the calibration values
    // we need two separate input tables one for keyboard and the other for the controller


    // the player can either use the controller or the keyboard he will toggle the buttons/inputs to what he
    // wants the button layout to be, for the vertical and horizontal these are either linked to the controller
    // or linked to the keyboard, if the keyboard then each needs to take in two buttons to determine its value
    // upon a request for a layout button setting we first see if using keyboard, if so then check if input is
    // a keyboard button, if so then 

    public InputAxisKey vertical = new InputAxisKey();
    public InputAxisKey horizontal = new InputAxisKey();
    public InputAxisMouse cameraVertical = new InputAxisMouse();
    public InputAxisMouse cameraHorizontal = new InputAxisMouse();


    // these are access by other scripts
    public InputButtonKey jump = new InputButtonKey();
    public InputButtonKey enable = new InputButtonKey();
    public InputButtonKey attack = new InputButtonKey();
    public InputKey speedUp = new InputKey();
    public Vector2 VHinput = new Vector2();
    public Vector2 CVHinput = new Vector2();



    public GameObject useKeyboardToggle;
    public GameObject keyBoardBox;
    public GameObject controllerBox;
    public GameObject inputBox;


    // Start is called before the first frame update
    void Start()
    {
        AssignInputNames();



        for (int i = 0; i < inputs.Length; i++)
        {
            //Debug.Log(inputs[i]);
        }

        /*
        keyBoardEnabled = true;
        keyBoardInvertCamera = true;
        controllerInvertCamera = true;

        f = 0.01f;
        b = 0.01f;
        l = 0.01f;
        r = 0.01f;
        cf = 0.01f;
        cb = 0.01f;
        cl = 0.01f;
        cr = 0.01f;

        vertical.assignedAxis = "Axis 4";
        vertical.assignedKey1 = "KeyBoard W";
        vertical.assignedKey2 = "KeyBoard S";

        horizontal.assignedAxis = "Axis 2";
        horizontal.assignedKey1 = "KeyBoard D";
        horizontal.assignedKey2 = "KeyBoard A";


        cameraVertical.assignedAxis = "Axis 8";
        cameraVertical.assignedMouse = "Mouse Y";

        cameraHorizontal.assignedAxis = "Axis 7";
        cameraHorizontal.assignedMouse = "Mouse X";


        jump.assignedButton = "Button 0";
        jump.assignedKey = "KeyBoard SPACE";

        enable.assignedButton = "Button 6";
        enable.assignedKey = "KeyBoard LEFT SHIFT";

        attack.assignedButton = "Button 2";
        attack.assignedKey = "KeyBoard B";

        speedUp.assignedKey = "KeyBoard E";
        */
        LoadInputSettings();
    }

    void AssignInputNames()
    {
        inputs = new string[83];
        inputs[0] = "Axis 1";
        inputs[1] = "Axis 2";
        inputs[2] = "Axis 3";
        inputs[3] = "Axis 4";
        inputs[4] = "Axis 5";
        inputs[5] = "Axis 6";
        inputs[6] = "Axis 7";
        inputs[7] = "Axis 8";
        inputs[8] = "Axis 9";
        inputs[9] = "Axis 10";
        inputs[10] = "Axis 11";
        inputs[11] = "Axis 12";
        inputs[12] = "Axis 13";
        inputs[13] = "Axis 14";
        inputs[14] = "Axis 15";
        inputs[15] = "Axis 16";
        inputs[16] = "Axis 17";
        inputs[17] = "Axis 18";
        inputs[18] = "Axis 19";
        inputs[19] = "Axis 20";
        inputs[20] = "Axis 21";
        inputs[21] = "Axis 22";
        inputs[22] = "Axis 23";
        inputs[23] = "Axis 24";
        inputs[24] = "Axis 25";
        inputs[25] = "Axis 26";
        inputs[26] = "Axis 27";
        inputs[27] = "Axis 28";

        inputs[28] = "KeyBoard A";
        inputs[29] = "KeyBoard B";
        inputs[30] = "KeyBoard C";
        inputs[31] = "KeyBoard D";
        inputs[32] = "KeyBoard E";
        inputs[33] = "KeyBoard F";
        inputs[34] = "KeyBoard G";
        inputs[35] = "KeyBoard H";
        inputs[36] = "KeyBoard I";
        inputs[37] = "KeyBoard J";
        inputs[38] = "KeyBoard K";
        inputs[39] = "KeyBoard L";
        inputs[40] = "KeyBoard M";
        inputs[41] = "KeyBoard N";
        inputs[42] = "KeyBoard O";
        inputs[43] = "KeyBoard P";
        inputs[44] = "KeyBoard Q";
        inputs[45] = "KeyBoard R";
        inputs[46] = "KeyBoard S";
        inputs[47] = "KeyBoard T";
        inputs[48] = "KeyBoard U";
        inputs[49] = "KeyBoard V";
        inputs[50] = "KeyBoard W";
        inputs[51] = "KeyBoard X";
        inputs[52] = "KeyBoard Y";
        inputs[53] = "KeyBoard Z";

        inputs[54] = "KeyBoard LEFT SHIFT";
        inputs[55] = "KeyBoard RIGHT SHIFT";
        inputs[56] = "KeyBoard SPACE";
        inputs[57] = "KeyBoard UP";
        inputs[58] = "KeyBoard DOWN";
        inputs[59] = "KeyBoard LEFT";
        inputs[60] = "KeyBoard RIGHT";
        inputs[61] = "KeyBoard ENTER";

        inputs[62] = "Mouse Button 0";

        inputs[63] = "Button 0";
        inputs[64] = "Button 1";
        inputs[65] = "Button 2";
        inputs[66] = "Button 3";
        inputs[67] = "Button 4";
        inputs[68] = "Button 5";
        inputs[69] = "Button 6";
        inputs[70] = "Button 7";
        inputs[71] = "Button 8";
        inputs[72] = "Button 9";
        inputs[73] = "Button 10";
        inputs[74] = "Button 11";
        inputs[75] = "Button 12";
        inputs[76] = "Button 13";
        inputs[77] = "Button 14";
        inputs[78] = "Button 15";
        inputs[79] = "Button 16";
        inputs[80] = "Button 17";
        inputs[81] = "Button 18";
        inputs[82] = "Button 19";



    }

    // Update is called once per frame
    void Update()
    {
        if (inputBoardActive)
        {
            vertical.currentValue = 0;
            horizontal.currentValue = 0;
            jump.currentValue = 0;
            enable.currentValue = 0;
            attack.currentValue = 0;
            cameraVertical.currentValue = 0;
            cameraHorizontal.currentValue = 0;
            speedUp.currentValue = 0;
            VHinput = Vector2.zero;
            CVHinput = Vector2.zero;

            if (calibrateInput)
            {
                float v = -Input.GetAxisRaw(vertical.assignedAxis);
                float h = Input.GetAxisRaw(horizontal.assignedAxis);
                if (h > r)
                {
                    r = h;
                }
                if (-h > l)
                {
                    l = -h;
                }
                if (v > f)
                {
                    f = v;
                }
                if (-v > b)
                {
                    b = -v;
                }
            }
            if (calibrateCamera)
            {
                float cv = -Input.GetAxisRaw(cameraVertical.assignedAxis);
                float ch = Input.GetAxisRaw(cameraHorizontal.assignedAxis);
                if (ch > cr)
                {
                    cr = ch;
                }
                if (-ch > cl)
                {
                    cl = -ch;
                }
                if (cv > cf)
                {
                    cf = cv;
                }
                if (-cv > cb)
                {
                    cb = -cv;
                }
            }


        }
        else
        {
            if (keyBoardEnabled)
            {
                vertical.currentValue = Input.GetAxisRaw(vertical.assignedKey1) - Input.GetAxisRaw(vertical.assignedKey2);
                horizontal.currentValue = Input.GetAxisRaw(horizontal.assignedKey1) - Input.GetAxisRaw(horizontal.assignedKey2);
                jump.currentValue = Input.GetAxisRaw(jump.assignedKey);
                enable.currentValue = Input.GetAxisRaw(enable.assignedKey);
                attack.currentValue = Input.GetAxisRaw(attack.assignedKey);
                cameraVertical.currentValue = Input.GetAxisRaw(cameraVertical.assignedMouse);
                cameraHorizontal.currentValue = Input.GetAxisRaw(cameraHorizontal.assignedMouse);
                speedUp.currentValue = Input.GetAxisRaw(speedUp.assignedKey);


                // now a little clean up
                VHinput = new Vector2(horizontal.currentValue, vertical.currentValue);
                VHinput.Normalize();
                if (speedUp.currentValue == 0)
                {
                    VHinput *= 0.5f;
                }



                CVHinput.x = cameraHorizontal.currentValue;
                if (keyBoardInvertCamera)
                {
                    CVHinput.y = -cameraVertical.currentValue;
                }
                else
                {
                    CVHinput.y = cameraVertical.currentValue;
                }

            }
            else
            {
                vertical.currentValue = -Input.GetAxisRaw(vertical.assignedAxis);
                horizontal.currentValue = Input.GetAxisRaw(horizontal.assignedAxis);
                jump.currentValue = Input.GetAxisRaw(jump.assignedButton);
                enable.currentValue = Input.GetAxisRaw(enable.assignedButton);
                attack.currentValue = Input.GetAxisRaw(attack.assignedButton);
                cameraVertical.currentValue = Input.GetAxisRaw(cameraVertical.assignedAxis);
                cameraHorizontal.currentValue = Input.GetAxisRaw(cameraHorizontal.assignedAxis);

                VHinput = new Vector2(horizontal.currentValue, vertical.currentValue);
                //Debug.Log("before " + VHinput.magnitude + " " + VHinput.x/r + " " + VHinput.y);
                if (VHinput.x > 0)
                {
                    VHinput.x /= r;
                }
                if (VHinput.x < 0)
                {
                    VHinput.x /= l;
                }
                if (VHinput.y > 0)
                {
                    VHinput.y /= f;
                }
                if (VHinput.y < 0)
                {
                    VHinput.y /= b;
                }
                //Debug.Log("after " + VHinput.magnitude + " " + VHinput.x + " " + VHinput.y);
                if (VHinput.magnitude > 0.75f)
                {
                    VHinput.Normalize();
                }
                //Debug.Log("after after " + VHinput.magnitude + " " + VHinput.x + " " + VHinput.y);
                CVHinput = new Vector2(cameraHorizontal.currentValue, cameraVertical.currentValue);

                if (CVHinput.x > 0)
                {
                    CVHinput.x /= cr;
                }
                if (CVHinput.x < 0)
                {
                    CVHinput.x /= cl;
                }
                if (CVHinput.y > 0)
                {
                    CVHinput.y /= cf;
                }
                if (CVHinput.y < 0)
                {
                    CVHinput.y /= cb;
                }

                if (controllerInvertCamera)
                {
                    CVHinput.y *= -1;
                }

                if (CVHinput.magnitude > 1)
                {
                    CVHinput.Normalize();
                }



            }
        }


        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("vertical " + vertical.assignedAxis + " " + vertical.assignedKey1 + " " + vertical.assignedKey2);
            Debug.Log("horizontal " + horizontal.assignedAxis + " " + horizontal.assignedKey1 + " " + horizontal.assignedKey2);
            Debug.Log("jump " + jump.assignedButton + " " + jump.assignedKey);
            Debug.Log("enable " + enable.assignedButton + " " + enable.assignedKey);
            Debug.Log("attack " + attack.assignedButton + " " + attack.assignedKey);
            Debug.Log("cameraVertical " + cameraVertical.assignedAxis + " " + cameraVertical.assignedMouse);
            Debug.Log("cameraHorizontal " + cameraHorizontal.assignedAxis + " " + cameraHorizontal.assignedMouse);
            Debug.Log("speedUp " + speedUp.assignedKey);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            inputBoardActive = !inputBoardActive;
            inputBox.SetActive(inputBoardActive);
        }
        
    }

    void OnGUI()
    {
        if(currentKey != null)
        {
            for(int i = 0; i < inputs.Length; i++)
            {
                if(Input.GetAxisRaw(inputs[i]) != 0)
                {
                    // we found the axis that was toggled
                    // therefore we want this axis to be assigned to the selected input button
                    if (keyBoardEnabled)
                    {
                        if (inputs[i].Contains("KeyBoard") || inputs[i].Contains("Mouse Button")) {
                            bool change = true;
                            if (currentKey.name == "Forward")
                            {
                                vertical.assignedKey1 = inputs[i];
                            }
                            else if (currentKey.name == "Backward")
                            {
                                vertical.assignedKey2 = inputs[i];
                            }
                            else if (currentKey.name == "Left")
                            {
                                horizontal.assignedKey2 = inputs[i];
                            }
                            else if (currentKey.name == "Right")
                            {
                                horizontal.assignedKey1 = inputs[i];
                            }
                            else if (currentKey.name == "Key Jump")
                            {
                                jump.assignedKey = inputs[i];
                            }
                            else if (currentKey.name == "Key Enable")
                            {
                                enable.assignedKey = inputs[i];
                            }
                            else if (currentKey.name == "Key Attack")
                            {
                                attack.assignedKey = inputs[i];
                            }
                            else if (currentKey.name == "Key Speed Up")
                            {
                                speedUp.assignedKey = inputs[i];
                            }
                            else
                            {
                                change = false;
                            }
                            if (change)
                            {
                                currentKey.transform.GetChild(0).GetComponent<Text>().text = inputs[i];
                                currentKey = null;
                            }
                            return;
                        }
                        else
                        {
                            // not a valid key
                        }
                    }
                    else if (!keyBoardEnabled)
                    {
                        if (inputs[i].Contains("Axis"))
                        {
                            bool change = true;
                            if (currentKey.name == "Vertical")
                            {
                                vertical.assignedAxis = inputs[i];
                            }
                            else if (currentKey.name == "Horizontal")
                            {
                                horizontal.assignedAxis = inputs[i];
                            }
                            else if (currentKey.name == "Camera Vertical")
                            {
                                cameraVertical.assignedAxis = inputs[i];
                            }
                            else if (currentKey.name == "Camera Horizontal")
                            {
                                cameraHorizontal.assignedAxis = inputs[i];
                            }
                            else
                            {
                                change = false;
                            }
                            if (change)
                            {
                                currentKey.transform.GetChild(0).GetComponent<Text>().text = inputs[i];
                                currentKey = null;
                            }
                        }
                        else if (inputs[i].Contains("Button"))
                        {
                            bool change = true;
                            if (currentKey.name == "Con Jump")
                            {
                                jump.assignedButton = inputs[i];
                            }
                            else if (currentKey.name == "Con Enable")
                            {
                                enable.assignedButton = inputs[i];
                            }
                            else if (currentKey.name == "Con Attack")
                            {
                                attack.assignedButton = inputs[i];
                            }
                            else
                            {
                                change = false;
                            }
                            if (change)
                            {
                                currentKey.transform.GetChild(0).GetComponent<Text>().text = inputs[i];
                                currentKey = null;
                            }
                        }
                        else
                        {
                            // not a valid button/axis
                        }
                    }
                }
            }
        }
    }

    public void ChangeKey(GameObject clicked)
    {
        currentKey = clicked;
    }

    public void ToggleKeyBoard()
    {
        if (keyBoardEnabledToggle.GetComponent<Toggle>().isOn)
        {
            keyBoardEnabled = true;
            keyBoardBox.SetActive(false);
            controllerBox.SetActive(true);
        }
        else
        {
            keyBoardEnabled = false;
            keyBoardBox.SetActive(true);
            controllerBox.SetActive(false);
        }
    }

    public void ToggleKeyboardInvertCamera()
    {
        if (keyBoardInvertCameraToggle.GetComponent<Toggle>().isOn)
        {
            keyBoardInvertCamera = true;
        }
        else
        {
            keyBoardInvertCamera = false;
        }
    }

    public void ToggleContollerInvertCamera()
    {
        if (controllerInvertCameraToggle.GetComponent<Toggle>().isOn)
        {
            controllerInvertCamera = true;
        }
        else
        {
            controllerInvertCamera = false;
        }
    }

    public void ToggleCalibrateInput()
    {
        calibrateInput = !calibrateInput;
    }

    public void ToggleCalibrateCamera()
    {
        calibrateCamera = !calibrateCamera;
    }

    public void SaveInputSettings()
    {
        // we need to save
        /*
        public bool keyBoardEnabled = true;
        public bool keyBoardInvertCamera = true;
        public bool controllerInvertCamera = true;
        public float f = 0.01f, b = 0.01f, l = 0.01f, r = 0.01f; // the calibration values
        public float cf = 0.01f, cb = 0.01f, cl = 0.01f, cr = 0.01f; // the calibration values
        */
        PlayerPrefs.SetInt("keyBoardEnabled", keyBoardEnabledToggle.GetComponent<Toggle>().isOn ? 1 : 0);
        PlayerPrefs.SetInt("keyBoardInvertCamera", keyBoardInvertCamera ? 1 : 0);
        PlayerPrefs.SetInt("controllerInvertCamera", controllerInvertCamera ? 1 : 0);

        PlayerPrefs.SetFloat("f", f);
        PlayerPrefs.SetFloat("b", b);
        PlayerPrefs.SetFloat("l", l);
        PlayerPrefs.SetFloat("r", r);
        PlayerPrefs.SetFloat("cf", cf);
        PlayerPrefs.SetFloat("cb", cb);
        PlayerPrefs.SetFloat("cl", cl);
        PlayerPrefs.SetFloat("cr", cr);

        PlayerPrefs.SetString("verticalA", vertical.assignedAxis);
        PlayerPrefs.SetString("verticalK1", vertical.assignedKey1);
        PlayerPrefs.SetString("verticalK2", vertical.assignedKey2);

        PlayerPrefs.SetString("horizontalA", horizontal.assignedAxis);
        PlayerPrefs.SetString("horizontalK1", horizontal.assignedKey1);
        PlayerPrefs.SetString("horizontalK2", horizontal.assignedKey2);

        PlayerPrefs.SetString("cameraVerticalA", cameraVertical.assignedAxis);
        PlayerPrefs.SetString("cameraVerticalM", cameraVertical.assignedMouse);

        PlayerPrefs.SetString("cameraHorizontalA", cameraHorizontal.assignedAxis);
        PlayerPrefs.SetString("cameraHorizontalM", cameraHorizontal.assignedMouse);

        PlayerPrefs.SetString("jumpB", jump.assignedButton);
        PlayerPrefs.SetString("jumpK", jump.assignedKey);

        PlayerPrefs.SetString("enableB", enable.assignedButton);
        PlayerPrefs.SetString("enableK", enable.assignedKey);

        PlayerPrefs.SetString("attackB", attack.assignedButton);
        PlayerPrefs.SetString("attackK", attack.assignedKey);

        PlayerPrefs.SetString("speedUp", speedUp.assignedKey);


        PlayerPrefs.Save();
    }

    public void LoadInputSettings()
    {
        keyBoardEnabledToggle.GetComponent<Toggle>().isOn = Convert.ToBoolean(PlayerPrefs.GetInt("keyBoardEnabled", 1));
        keyBoardInvertCameraToggle.GetComponent<Toggle>().isOn = Convert.ToBoolean(PlayerPrefs.GetInt("keyBoardInvertCamera", 1));
        controllerInvertCameraToggle.GetComponent<Toggle>().isOn = Convert.ToBoolean(PlayerPrefs.GetInt("controllerInvertCamera", 1));

        f = PlayerPrefs.GetFloat("f", 0.01f);
        b = PlayerPrefs.GetFloat("b", 0.01f);
        l = PlayerPrefs.GetFloat("l", 0.01f);
        r = PlayerPrefs.GetFloat("r", 0.01f);
        cf = PlayerPrefs.GetFloat("cf", 0.01f);
        cb = PlayerPrefs.GetFloat("cb", 0.01f);
        cl = PlayerPrefs.GetFloat("cl", 0.01f);
        cr = PlayerPrefs.GetFloat("cr", 0.01f);

        vertical.assignedAxis = PlayerPrefs.GetString("verticalA", "Axis 4");
        vertical.assignedKey1 = PlayerPrefs.GetString("verticalK1", "KeyBoard W");
        vertical.assignedKey2 = PlayerPrefs.GetString("verticalK2", "KeyBoard S");

        horizontal.assignedAxis = PlayerPrefs.GetString("horizontalA", "Axis 2");
        horizontal.assignedKey1 = PlayerPrefs.GetString("horizontalK1", "KeyBoard D");
        horizontal.assignedKey2 = PlayerPrefs.GetString("horizontalK2", "KeyBoard A");


        cameraVertical.assignedAxis = PlayerPrefs.GetString("cameraVerticalA", "Axis 8");
        cameraVertical.assignedMouse = PlayerPrefs.GetString("cameraVerticalM", "Mouse Y");

        cameraHorizontal.assignedAxis = PlayerPrefs.GetString("cameraHorizontalA", "Axis 7");
        cameraHorizontal.assignedMouse = PlayerPrefs.GetString("cameraHorizontalM", "Mouse X");


        jump.assignedButton = PlayerPrefs.GetString("jumpB", "Button 0");
        jump.assignedKey = PlayerPrefs.GetString("jumpK", "KeyBoard SPACE");

        enable.assignedButton = PlayerPrefs.GetString("enableB", "Button 6");
        enable.assignedKey = PlayerPrefs.GetString("enableK", "KeyBoard LEFT SHIFT");

        attack.assignedButton = PlayerPrefs.GetString("attackB", "Button 2");
        attack.assignedKey = PlayerPrefs.GetString("attackK", "KeyBoard B");

        speedUp.assignedKey = PlayerPrefs.GetString("speedUp", "KeyBoard E");

        forwardText.GetComponent<Text>().text = vertical.assignedKey1;
        backwardText.GetComponent<Text>().text = vertical.assignedKey2;
        leftText.GetComponent<Text>().text = horizontal.assignedKey1;
        rightText.GetComponent<Text>().text = horizontal.assignedKey2;
        kJumpText.GetComponent<Text>().text = jump.assignedKey;
        kEnableText.GetComponent<Text>().text = enable.assignedKey;
        kAttackText.GetComponent<Text>().text = attack.assignedKey;
        speedUpText.GetComponent<Text>().text = speedUp.assignedKey;
        verticalText.GetComponent<Text>().text = vertical.assignedAxis;
        horizontalText.GetComponent<Text>().text = horizontal.assignedAxis;
        cJumpText.GetComponent<Text>().text = jump.assignedButton;
        cEnableText.GetComponent<Text>().text = enable.assignedButton;
        cAttackText.GetComponent<Text>().text = attack.assignedButton;
        cameraVerticalText.GetComponent<Text>().text = cameraVertical.assignedAxis;
        cameraHorizontalText.GetComponent<Text>().text = cameraHorizontal.assignedAxis;
    }

    public class InputAxisKey // an input that is represented by two keys on a keyboard but a single axis on a controller
    {
        public string assignedKey1; // keyboard value
        public string assignedKey2; // keyboard value
        public string assignedAxis; // controller value
        public float currentValue; // the value of the input on a given frame
    }

    public class InputButtonKey // an input that is represented by one key on a keyboard and a single button on a controller
    {
        public string assignedKey;
        public string assignedButton; // the name of the assigned "input panel" axis (axis 1-28, keyboard a-z, shift, space, etc.)
        public float currentValue; // the value of the input on a given frame
    }

    public class InputKey // an input that is represented by one key on a keyboard
    {
        public string assignedKey;
        public float currentValue; // the value of the input on a given frame
    }

    public class InputAxisMouse
    {
        public string assignedAxis;
        public string assignedMouse;
        public float currentValue; // the value of the input on a given frame
    }
    
}
