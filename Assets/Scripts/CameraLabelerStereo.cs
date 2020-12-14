using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

public class CameraLabelerStereo : MonoBehaviour
{
    //GameObject to detect
    public GameObject Object;

    //Two cameras for exporting
    GameObject camera1;         //Left Camera
    GameObject camera2;         //Right Camera

    //Variables for Video Recording
    float captureDelay = 1f;
    bool isRecordingToPrepare;
    bool isRecordingDone;
    bool isRecorderOn;
    float _timeOut;

    public Vector2Int resolution = new Vector2Int(800, 800);
    public int frameRate = 20;

    string outputPathFolder;
    public bool initVideoNameIndex;
    int takeNum;

    RecorderController recorderController;
    RecorderControllerSettings recorderControlSettings;
    MovieRecorderSettings movieRecorder1;
    MovieRecorderSettings movieRecorder2;

    public float cameraDistance = 0.5f;

    //Variables for object rotation
    GameObject pivot;
    float xrot;
    float yrot;
    public float xRotationAmount = 10f;
    public float yRotationAmount = 10f;

    //Variables for Text Output
    string textPath1;
    string textPath2;
    string outputContent1;
    string outputContent2;
    int frame;
    string takeNumString;
    Bounds bound;

    void Start()
    {
        //The code works even if the user just sets the name of the object to "Object"
        if (!Object)
        {
            Object = GameObject.Find("Object");
        }

        //SETTINGS FOR OBJECT ROTATION
        bound = GetBounds(Object);
        Object.transform.position = Vector3.zero;
        pivot = GameObject.Find("Pivot");
        pivot.transform.position = bound.center;           //Need to verify if there could be any other renderer than MeshRenderer
        Object.transform.SetParent(pivot.transform);
        xrot = 0;
        yrot = 0;

        //LEFT & RIGHT CAMERA SETTINGS
        camera1 = transform.GetChild(0).gameObject;
        camera2 = transform.GetChild(1).gameObject;
        camera1.transform.Translate(-cameraDistance, 0, 0);
        camera2.transform.Translate(cameraDistance, 0, 0);
        camera1.transform.LookAt(pivot.transform);
        camera2.transform.LookAt(pivot.transform);

        //MANUALLY SET TAKE NUMBER
        if (initVideoNameIndex)
        {
            PlayerPrefs.SetInt("Take", 0);
        }

        frame = 0;
        isRecordingToPrepare = true;
        isRecordingDone = false;
        isRecorderOn = false;
        _timeOut = captureDelay;

        takeNum = PlayerPrefs.GetInt("Take", 0);

        outputPathFolder = Application.dataPath + "/Recordings_Stereo/";       // ~Assets/Recordings

        var renderTexture1 = new RenderTexture(resolution.x, resolution.y, 32);
        var renderTexture2 = new RenderTexture(resolution.x, resolution.y, 32);
        camera1.GetComponent<Camera>().targetTexture = renderTexture1;
        camera2.GetComponent<Camera>().targetTexture = renderTexture2;

        //GLOBAL UNITY RECORDER SETTINGS
        RecorderOptions.VerboseMode = true;
        recorderControlSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        recorderControlSettings.FrameRatePlayback = FrameRatePlayback.Constant;
        recorderControlSettings.FrameRate = frameRate;
        recorderControlSettings.CapFrameRate = true;

        //SETTINGS FOR FIRST MOVIE RECORDER 
        movieRecorder1 = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        movieRecorder1.Enabled = true;
        movieRecorder1.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MP4;
        movieRecorder1.OutputFile = outputPathFolder + DefaultWildcard.Take + "_Left"; //  Assets/Recordings/<Take>
        movieRecorder1.VideoBitRateMode = VideoBitrateMode.High;
        movieRecorder1.Take = takeNum;
        movieRecorder1.AudioInputSettings.PreserveAudio = false;
        movieRecorder1.ImageInputSettings = new RenderTextureInputSettings
        {
            OutputWidth = resolution.x,
            OutputHeight = resolution.y,
            RenderTexture = renderTexture1,
        };

        //SETTINGS FOR SECOND MOVIE RECORDER 
        movieRecorder2 = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        movieRecorder2.Enabled = true;
        movieRecorder2.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MP4;
        movieRecorder2.OutputFile = outputPathFolder + DefaultWildcard.Take + "_Right"; //  Assets/Recordings/<Take>
        movieRecorder2.VideoBitRateMode = VideoBitrateMode.High;
        movieRecorder2.Take = takeNum;
        movieRecorder2.AudioInputSettings.PreserveAudio = false;
        movieRecorder2.ImageInputSettings = new RenderTextureInputSettings
        {
            OutputWidth = resolution.x,
            OutputHeight = resolution.y,
            RenderTexture = renderTexture2,
        };

        //Add two Recorders to RecorderController
        recorderControlSettings.AddRecorderSettings(movieRecorder1);
        recorderControlSettings.AddRecorderSettings(movieRecorder2);

        recorderController = new RecorderController(recorderControlSettings);

        if (takeNum < 10)
        {
            takeNumString = "00" + takeNum;
        }
        else if (movieRecorder1.Take < 100)
        {
            takeNumString = "0" + takeNum;
        }
        else
        {
            takeNumString = takeNum.ToString();
        }

        //Generate text file to be exported
        textPath1 = outputPathFolder + takeNumString + "_Left.txt";
        textPath2 = outputPathFolder + takeNumString + "_Right.txt";
        if (File.Exists(textPath1)) File.Delete(textPath1); //Delete file if text file already exists
        File.WriteAllText(textPath1, "");
    }

    // Update is called once per frame
    void Update()
    {
        if (isRecorderOn)
        {
            //Write Content in txt file
            outputContent1 = takeNumString + " " + frame + " ";
            outputContent2 = takeNumString + " " + frame + " ";

            //Boundbox & Rotation Info
            outputContent1 += BoundingBox(camera1);
            outputContent1 += xrot + " " + yrot;
            outputContent2 += BoundingBox(camera2);
            outputContent2 += xrot + " " + yrot;

            outputContent1 += "\n";
            outputContent2 += "\n";
            File.AppendAllText(textPath1, outputContent1);
            File.AppendAllText(textPath2, outputContent2);
            frame++;

            //Rotate Object by x axis
            pivot.transform.Rotate(xRotationAmount, 0, 0);
            xrot += xRotationAmount;

            if (xrot >= 360f)
            {
                xrot -= 360f;

                //Rotate Object by y axis
                pivot.transform.Rotate(0, yRotationAmount, 0);
                yrot += yRotationAmount;

                if (yrot >= 360f)
                {
                    //Object rotation is finished.
                    isRecorderOn = false;
                    isRecordingDone = true;
                }
            }

            //Update bounds after rotation
            bound = GetBounds(Object);
        }
        else
        {
            if (isRecordingDone)
            {
                _timeOut = 1;
                recorderController.StopRecording();
                isRecordingDone = false;
                Debug.Log("Stopped Recording");
                PlayerPrefs.SetInt("Take", takeNum + 1);
            }
            else
            {
                if (_timeOut > 0)
                {
                    _timeOut -= Time.deltaTime;
                }
                else if (isRecordingToPrepare)
                {
                    recorderController.PrepareRecording();
                    recorderController.StartRecording();
                    Debug.Log("Starting Recording");
                    isRecorderOn = true;
                    isRecordingToPrepare = false;
                }
                else
                {
                    //once everything is done automatically exit the simulation
                    EditorApplication.isPlaying = false;
                    return;
                }
            }
        }
    }

    string BoundingBox(GameObject camera)
    {
        float min_x = resolution.x, min_y = resolution.y, max_x = 0f, max_y = 0f;
        Vector2 screenPos;

        for (int i = -1; i <= 1; i += 2)
        {
            for (int j = -1; j <= 1; j += 2)
            {
                for (int k = -1; k <= 1; k += 2)
                {
                    screenPos = camera.GetComponent<Camera>().WorldToScreenPoint(new Vector3(bound.center.x + (i * bound.extents.x), bound.center.y + (j * bound.extents.y), bound.center.z + (k * bound.extents.z)));

                    /*
                    if (screenPos.x < 0f || screenPos.x > frameSize.x || screenPos.y < 0f || screenPos.y > frameSize.y)
                    {
                        return "(Frame " + frame + ": screenPos out of bounds; " + screenPos.x + " " + screenPos.y + ") ";
                    }
                    */

                    //If screenPos is out of bounds of the screen, cap the value to the edge of screen
                    if (screenPos.x < 0f)
                    {
                        screenPos.x = 0;
                    }
                    if (screenPos.x > resolution.x)
                    {
                        screenPos.x = resolution.x;
                    }
                    if (screenPos.y < 0f)
                    {
                        screenPos.y = 0;
                    }
                    if (screenPos.y > resolution.y)
                    {
                        screenPos.y = resolution.y;
                    }

                    if (min_x > screenPos.x)
                    {
                        min_x = screenPos.x;
                    }
                    if (min_y > (resolution.y - screenPos.y))
                    {
                        min_y = resolution.y - screenPos.y;
                    }
                    if (max_x < screenPos.x)
                    {
                        max_x = screenPos.x;
                    }
                    if (max_y < (resolution.y - screenPos.y))
                    {
                        max_y = resolution.y - screenPos.y;
                    }
                }
            }
        }

        string outputContent = min_x + " " + min_y + " " + max_x + " " + max_y + " ";
        return outputContent;
    }

    Bounds GetBounds(GameObject obj)
    {
        bound = Object.GetComponent<MeshRenderer>().bounds;

        //If the gameobject to detect doesn't use MeshRenderer, bounds can be manually set with a box collider instead.
        if (bound == null)
        {
            bound = Object.GetComponent<BoxCollider>().bounds;
        }

        return bound;
    }
}
