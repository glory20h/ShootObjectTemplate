using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

public class CameraLabeler : MonoBehaviour
{
    //GameObject to detect
    public GameObject Object;

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
    MovieRecorderSettings movieRecorder;

    //Variables for object rotation
    GameObject pivot;
    float xrot;
    float yrot;
    public float xRotationAmount = 10f;
    public float yRotationAmount = 10f;
    
    //Variables for Text Output
    string textPath;
    string outputContent;
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
        //Debug.Log("First Take : " + takeNum);

        outputPathFolder = Application.dataPath + "/Recordings/";       // ~Assets/Recordings

        var renderTexture = new RenderTexture(resolution.x, resolution.y, 32);
        GetComponent<Camera>().targetTexture = renderTexture;

        //GLOBAL UNITY RECORDER SETTINGS
        RecorderOptions.VerboseMode = true;
        recorderControlSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        recorderControlSettings.FrameRatePlayback = FrameRatePlayback.Constant;
        recorderControlSettings.FrameRate = frameRate;
        recorderControlSettings.CapFrameRate = true;

        //MOVIE RECORDER SETTINGS
        movieRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        movieRecorder.Enabled = true;
        movieRecorder.OutputFormat = MovieRecorderSettings.VideoRecorderOutputFormat.MP4;
        movieRecorder.OutputFile = outputPathFolder + DefaultWildcard.Take; //  Assets/Recordings/<Take>
        movieRecorder.VideoBitRateMode = VideoBitrateMode.High;
        movieRecorder.Take = takeNum;
        movieRecorder.AudioInputSettings.PreserveAudio = false;
        movieRecorder.ImageInputSettings = new RenderTextureInputSettings
        {
            OutputWidth = resolution.x,
            OutputHeight = resolution.y,
            RenderTexture = renderTexture,
        };

        recorderControlSettings.AddRecorderSettings(movieRecorder);

        recorderController = new RecorderController(recorderControlSettings);

        if(movieRecorder.Take < 10)
        {
            takeNumString = "00" + movieRecorder.Take;
        }
        else if(movieRecorder.Take < 100)
        {
            takeNumString = "0" + movieRecorder.Take;
        }
        else
        {
            takeNumString = movieRecorder.Take.ToString();
        }

        //Generate text file to be exported
        textPath = outputPathFolder + takeNumString + ".txt";
        if (File.Exists(textPath)) File.Delete(textPath); //Delete file if text file already exists
        File.WriteAllText(textPath, "");
    }

    // Update is called once per frame
    void Update()
    {
        if (isRecorderOn)
        {
            //Write Content in txt file
            outputContent = takeNumString + " " + frame + " ";

            //Boundbox & Rotation Info
            outputContent += BoundingBox(frame);
            outputContent += xrot + " " + yrot;

            outputContent += "\n";
            File.AppendAllText(textPath, outputContent);
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
                else if(isRecordingToPrepare)
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

        /*
        if (_timeOut > 0)
        {
            _timeOut -= Time.deltaTime;

            //Only write when the camera is on
            if (isRecorderOn)
            {
                //UpdateVariables
                //Get TopLeft/BottomRight Coordinates
                //topLeft = Camera.main.WorldToScreenPoint(Top_Left_Detector.position);
                //bottomRight = Camera.main.WorldToScreenPoint(Bottom_Right_Detector.position);

                //Write Content in txt file
                //outputContent = takeNumString + " " + frame + " " + topLeft.x + " " + (1080f - topLeft.y) + " " + bottomRight.x + " " + (1080f - bottomRight.y) + " " + pedestrianBehavior + " " + viewTarget + "\n";
                outputContent = takeNumString + " " + frame + " ";

                outputContent += "\n";
                File.AppendAllText(textPath, outputContent);
                frame++;
            }
        }
        else
        {
            if (isRecordingToPrepare)
            {
                //once the time set in the shutterdelay has passed start the recording and set the timer to the duration specified
                _timeOut = captureDuration;
                recorderController.PrepareRecording();
                recorderController.StartRecording();
                Debug.Log("Starting Recording");
                isRecorderOn = true;
                isRecordingToPrepare = false;
            }
            else if (!isRecordingDone)
            {
                _timeOut = 1;
                recorderController.StopRecording();
                isRecordingDone = true;
                Debug.Log("Stopped Recording");
                PlayerPrefs.SetInt("Take", takeNum + 1);
            }
            else
            {
                EditorApplication.isPlaying = false;
                return;
            }
        }
        */
    }

    string BoundingBox(int frame)
    {
        float min_x = resolution.x, min_y = resolution.y, max_x = 0f, max_y = 0f;
        Vector2 screenPos;
        
        for (int i = -1; i <= 1; i += 2)
        {
            for (int j = -1; j <= 1; j += 2)
            {
                for (int k = -1; k <= 1; k += 2)
                {
                    screenPos = GetComponent<Camera>().WorldToScreenPoint(new Vector3(bound.center.x + (i * bound.extents.x), bound.center.y + (j * bound.extents.y), bound.center.z + (k * bound.extents.z)));

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

                    if(min_x > screenPos.x)
                    {
                        min_x = screenPos.x;
                    }
                    if(min_y > (resolution.y - screenPos.y))
                    {
                        min_y = resolution.y - screenPos.y;
                    }
                    if(max_x < screenPos.x)
                    {
                        max_x = screenPos.x;
                    }
                    if(max_y < (resolution.y - screenPos.y))
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
