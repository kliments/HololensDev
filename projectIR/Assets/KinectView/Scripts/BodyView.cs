using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System.IO;
using System;
using System.Linq;

public class BodyView : MonoBehaviour
{
	//TODO variables either small capital or not
    [Header("Materials")]
    public Material BoneMaterial;
    public Material highlight;

    [Header("ProjectIR Variables")]
    public GameObject BodySourceManager;
    public string avatarAssetName;

    [Header("SoundClips")]
    public AudioClip backSound;
    public AudioClip positiveSound;

    [Header("GameObjects for Instructions and Feedback")]
    public GameObject Instruction;
    public GameObject Animations;



    public static Vector3 backstraight;
    public static ulong UserID = 0;

    private List<Vector3> averageSpineShoulder = new List<Vector3>();
    private List<Vector3> averageSpineBase = new List<Vector3>();

    [Header("Sprites for the Instructions")]
    public Sprite task;
    public Sprite badFeedback;
    public Sprite goodFeedback;
    public Sprite noMovement;
    public Sprite noCalibration;

    [Header("Animations")]
    public GameObject sketchAnimation;
    public GameObject klimentBends;
    
    [Header("Statistics")]
    public GameObject Statistics;
    public GameObject GoodCube;
    public GameObject BadCube;
    public GameObject Badstatistics;
    public GameObject Goodstatistics;
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();

    private BodyDataConverter _BodyDataConverter;
    private BodyDataReceiver _BodyDataReceiver;
    private int FeedbackProvided = 0;
    private Boolean CalibrationActive = false;
    private Boolean CalibrationDone = false;
    private Boolean RightKneeBend = false;
    private Boolean LeftKneeBend = false;
    private float BadMovements = 0;
    private float GoodMovements = 0;
    private int LearningStage = 0;
    private int TaskCounter = 0;

    private Boolean TaskAktive = false;
    private Boolean TaskDone = false;
    private int ActualAngle;
    private float KneeAngle;
    private float KneeAngleLeft;
    private float KneeAngleRight;
    private int GeneralCounter = 0;
    private int GoodMoveCounter = 0;
    private int BadMoveCounter = 0;
    private int FrameCounter = 0;
    int standingCounter = 0;
    //private HoloUser User;
    Vector3 spineShoulder ;
    Vector3 spineBase ;
    Vector3 hipRight;
    Vector3 kneeRight;
    Vector3 ankleRight;
    Vector3 hipLeft;
    Vector3 kneeLeft;
    Vector3 ankleLeft;
    



    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },

        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },

        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };


    // Jill Zombie Avatar from Mixamo

    private Dictionary<string, string> _RigMap = new Dictionary<string, string>()
    {
        {"SpineBase", "Hips"},
        {"KneeLeft", "Hips/RightUpLeg"},
        {"KneeRight", "Hips/LeftUpLeg"},
        {"AnkleLeft", "Hips/RightUpLeg/RightLeg"},
        {"AnkleRight", "Hips/LeftUpLeg/LeftLeg"},
        //{"FootLeft", "Hips/RightUpLeg/RightLeg/RightFoot"},
        //{"FootRight", "Hips/LeftUpLeg/LeftLeg/LeftFoot"},

        {"SpineMid", "Hips/Spine"},
        {"SpineShoulder", "Hips/Spine/Spine1/Spine2"},
        {"ShoulderLeft", "Hips/Spine/Spine1/Spine2/RightShoulder"},
        {"ShoulderRight", "Hips/Spine/Spine1/Spine2/LeftShoulder"},
        {"ElbowLeft", "Hips/Spine/Spine1/Spine2/RightShoulder/RightArm"},
        {"ElbowRight", "Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm"},
        {"WristLeft", "Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm"},
        {"WristRight", "Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm"},

        //{"HandLeft", "Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand"},
        //{"HandRight", "Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm/LeftHand"},

        {"Neck", "Hips/Spine/Spine1/Spine2/Neck"},
        {"Head", "Hips/Spine/Spine1/Spine2/Neck/Neck1/Head"},

    };

    private Dictionary<string, Quaternion> _RigMapOffsets = new Dictionary<string, Quaternion>()
    {
        {"SpineBase", Quaternion.Euler(0.0f,0.0f, 0.0f)},
        {"KneeLeft", Quaternion.Euler(0.0f, 90.0f, 0.0f)},
        {"KneeRight", Quaternion.Euler(0.0f, -90.0f, 0.0f)},
        {"AnkleLeft", Quaternion.Euler(0.0f, 90.0f, 0.0f)},
        {"AnkleRight", Quaternion.Euler(0.0f, -90.0f, 0.0f)},
        //{"FootLeft", Quaternion.Euler(225.0f, 0.0f, 0.0f)},
        //{"FootRight", Quaternion.Euler(225.0f, 0.0f, 0.0f)},

        {"SpineMid",Quaternion.Euler(0.0f, 0.0f, 0.0f)},
        {"SpineShoulder",Quaternion.Euler(0.0f, 0.0f, 0.0f)},
        {"ShoulderLeft", Quaternion.Euler(0.0f, 90.0f, 0.0f)},
        {"ShoulderRight", Quaternion.Euler(0.0f, -90.0f, 0.0f)},
        {"ElbowLeft", Quaternion.Euler(0.0f, 180.0f, 0.0f)},
        {"ElbowRight", Quaternion.Euler(0f, -180.0f, 0.0f)},
        {"WristLeft", Quaternion.Euler(0.0f, 90.0f, 0.0f)},
        {"WristRight", Quaternion.Euler(0.0f, -90.0f, 0.0f)},

        //{"HandLeft", Quaternion.Euler(0.0f, 90.0f, 0.0f)},
        //{"HandRight", Quaternion.Euler(0.0f, 0.0f, 0.0f)},

        {"Neck", Quaternion.Euler(0.0f, 0.0f, 0.0f)},
        {"Head", Quaternion.Euler(0.0f, 0.0f, 0.0f)},

    };

    // Zombie Avatar
    /*
  private Dictionary<string, string> _RigMap = new Dictionary<string, string>()
  {
      {"SpineBase", "Bip01"},
      {"KneeRight", "Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 L Thigh"},
      {"KneeLeft", "Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 R Thigh"},
      {"AnkleRight", "Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 L Thigh/Bip01 L Calf"},
      {"AnkleLeft", "Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 R Thigh/Bip01 R Calf"},
      {"FootRight", "Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 L Thigh/Bip01 L Calf/Bip01 L Foot"},
      {"FootLeft", "Bip01/Bip01 Pelvis/Bip01 Spine/Bip01 R Thigh/Bip01 R Calf/Bip01 R Foot"},
  };
   */
    void Update()
    {
        if (BodySourceManager == null)
        {
            return;
        }

        Dictionary<ulong, Vector3[]> bodies_pos = null;
        Dictionary<ulong, Quaternion[]> bodies_rot = null;

        _BodyDataConverter = BodySourceManager.GetComponent<BodyDataConverter>();
        if (_BodyDataConverter == null)
        {
            _BodyDataReceiver = BodySourceManager.GetComponent<BodyDataReceiver>();
            if (_BodyDataReceiver == null)
            {
                Debug.Log("No body data receiver");
                return;
            }
            else
            {
                Debug.Log("Getting bodies position orientation data");
                bodies_pos = _BodyDataReceiver.GetPosData();
                bodies_rot = _BodyDataReceiver.GetRotData();
                Debug.Log(bodies_pos + " " + bodies_rot);
            }
        }
        else
        {
            //bodies = _BodyDataConverter.GetData();
        }

        if (bodies_pos == null || bodies_rot == null)
        {
            Debug.Log("No bodies position orientation data");
            return;
        }

        List<ulong> trackedIDs = new List<ulong>(bodies_pos.Keys);
        List<ulong> knownIDs = new List<ulong>(_Bodies.Keys);
        foreach (ulong trackingID in knownIDs)
        {

            if (!trackedIDs.Contains(trackingID))
            {
                Destroy(_Bodies[trackingID]);
                _Bodies.Remove(trackingID);
            }
        }


        // if calibration is not yet done and calibration is active, teh counter is started
        if (CalibrationActive == true && CalibrationDone == false && FrameCounter < 240)
        {
            GetComponent<TextMesh>().text = "Calibration in process";
            {
                foreach (ulong trackingID in bodies_pos.Keys)
                {

                    // doing the calibration only if the person is in the marked area
                    if (bodies_pos[trackingID][0].z >= 8 
                        && bodies_pos[trackingID][0].z <= 10 
                        && bodies_pos[trackingID][0].x >= -1 
                        && bodies_pos[trackingID][0].x <= 1)
                    {
                        // saves trackingID of person in marked area as UserID
                        UserID = trackingID;
                        break;
                    }
                    else
                    {
                        // if user is out of area: change instruction to specific sprite
                        ChangeInstruction(noCalibration);
                        ShowInstruction();
                        CalibrationActive = false;
                        //hide "Calibration in process"
                        GetComponent<TextMesh>().text = "";
                    }
                }
            }
       
            // adds the current position of spineShoulder/ SpineBase to the list in order to calculate average Vector
            averageSpineShoulder.Add(bodies_pos[UserID][20]);
            averageSpineBase.Add(bodies_pos[UserID][0]);

            FrameCounter++;

            // after 4 seconds...
            if (FrameCounter == 240)
            {
                // the SaveCalibration function calculates the average Vector and saves the backStraight Vector
                SaveCalibration(bodies_pos[UserID][20], bodies_pos[UserID][0]);
                GetComponent<TextMesh>().text = "";
                FrameCounter++;
                // an information for the user is shown, that the calibration is done
                
            }


        }

       
        if (CalibrationDone == true)
        {
            // skeleton for user ID is created and updated
            foreach (ulong trackingID in bodies_pos.Keys)
            {
                if (!_Bodies.ContainsKey(trackingID) &&
                    trackingID == UserID)

                {
                    _Bodies[trackingID] = CreateBodyObject(trackingID);
                }

                RefreshBodyObject(UserID, bodies_pos, bodies_rot);
            }

            Debug.Log("Calibration is done.");
            if(TaskDone == false) {
                CreateTask();
                ThresholdsCalculation(UserID, bodies_pos, GetComponent<TextMesh>());
            }
           
        }
        
    }

    private GameObject CreateBodyObject(ulong id)
    {
        Debug.Log("Created Avatar Object");
        GameObject body = new GameObject("Body:" + id);
        GameObject avatar = Instantiate(Resources.Load(avatarAssetName, typeof(GameObject))) as GameObject;
        avatar.transform.parent = body.transform;
        avatar.name = "Avatar";
        //adds tags for different bodyparts in order to highlight them
        GameObject l1 = GameObject.Find("l1");
        l1.tag = "Back";
        GameObject l02 = GameObject.Find("l02");
        l02.tag = "Back";
        GameObject l2 = GameObject.Find("l2");
        l2.tag = "Back";
        GameObject l3 = GameObject.Find("l3");
        l3.tag = "Back";
        GameObject l4 = GameObject.Find("l4");
        l4.tag = "Back";
        GameObject l5 = GameObject.Find("l5");
        l5.tag = "Back";
        return body;
    }

    /*    private void SetAvatarScale(GameObject bodyObject)
        {

            Transform avatar = bodyObject.transform.FindChild("Avatar");
            if (avatar.localScale.x != 1)
            {
                return;
            }

            //Scale avatar based on torso distance
            Transform hips = avatar.FindChild("Hips");
            Transform spineBase = bodyObject.transform.FindChild("SpineBase");
            Transform spineShoulder = bodyObject.transform.FindChild("SpineShoulder");
            float bodyScale = Vector3.Magnitude(spineShoulder.position - spineBase.position);
            Transform neck = avatar.FindChild("Hips/Spine/Spine1/Spine2/Neck/Neck1/Head");
            float avatarScale = Vector3.Magnitude(neck.position - hips.position);
            float scaleFactor = bodyScale / avatarScale;
            avatar.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

        }*/

    private void RefreshBodyObject(ulong id, Dictionary<ulong, Vector3[]> bodies_pos, Dictionary<ulong, Quaternion[]> bodies_rot)
    {

        Debug.Log("Updating Avatar Object");
        GameObject bodyObject = _Bodies[id];
        TextMesh infoText = GetComponent<TextMesh>();



        Transform avatar = bodyObject.transform.FindChild("Avatar");
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            if (_BoneMap.ContainsKey(jt))
            {
                if (_RigMap.ContainsKey(jt.ToString()))
                {
                    Transform avatarItem = avatar.FindChild(_RigMap[jt.ToString()]);

                    if (jt.ToString() == "SpineBase")
                    {
                        avatarItem.position = bodies_pos[id][(int)jt];
                    }
                    avatarItem.rotation = bodies_rot[id][(int)jt] * _RigMapOffsets[jt.ToString()];

                }


            }

        }


    }


    // Function to change the Instruction Text
    private void ChangeInstruction(Sprite instr)
    {
        Debug.Log("Tries to change Sprite to new instruction");
        SpriteRenderer sp = Instruction.transform.Find("New Sprite").GetComponent<SpriteRenderer>();
        sp.sprite = instr;
    }

    // Funtion to show the instructions of the Calibration
    private void ShowInstruction()
    {
        Debug.Log("Show Instruction");
        Instruction.SetActive(true);
    }

    // Function to disable instruction of Calibration and then call StartCalibration
    // (StartCalibration() cannot be called directly via VoiceRecognitiopn because neewds parameters
    public void HideInstruction()
    {
        Debug.Log("Hide Instruction");
        Instruction.SetActive(false);
        CalibrationActive = true;

    }

    // Function to save the calibration results and calculate the average
    // puclic because Keyword Manager has to access teh function

    public void SaveCalibration(Vector3 spineShoulder, Vector3 spineBase)
    {
       
        Debug.Log("Start Calibration of User" + UserID);
        Vector3 averagespineShoulder = new Vector3(
        averageSpineShoulder.Average(x => x.x),
        averageSpineShoulder.Average(x => x.y),
        averageSpineShoulder.Average(x => x.z));
        Vector3 averagespineBase = new Vector3(
        averageSpineBase.Average(x => x.x),
        averageSpineBase.Average(x => x.y),
        averageSpineBase.Average(x => x.z));

        backstraight = spineBase - spineShoulder;
        Debug.Log("Vector standing straight: " + backstraight);
        CalibrationActive = false;
        CalibrationDone = true;
    }



    //Helperfunction to calculate the Angle between two Vectors.

    private static double AngleBetweenTwoVectors(Vector3 vectorA, Vector3 vectorB)
    {
        double dotProduct;
        vectorA.Normalize();
        vectorB.Normalize();
        dotProduct = Vector3.Dot(vectorA, vectorB);

        return (double)Math.Acos(dotProduct) / Math.PI * 180;
    }

    //Function to provide visual Feedback after finishing the task
    private void VisualSummativeFeedback()
    {
        Material[] highlights = new Material[] { highlight };
        ScaleStatistics();
        if (BadMoveCounter > 0)
        {
            GameObject[] bodyparts = GameObject.FindGameObjectsWithTag("Back");
            foreach (GameObject bodypart in bodyparts) 
            {
                bodypart.GetComponent<SkinnedMeshRenderer>().materials[0] = highlight;
            }
            ChangeInstruction(badFeedback);
            ShowInstruction();
            if (TaskCounter >= 3)
            { 
                Statistics.SetActive(true);
            }
            if(LearningStage > 0)
            { 
                LearningStage--;
            }
        }
        else if (GoodMoveCounter > 0)
        {
            ChangeInstruction(goodFeedback);
            ShowInstruction();
            if (TaskCounter >= 3)
            {
                Statistics.SetActive(true);
            }
            if (LearningStage < 2)
            { 
                LearningStage++;
            }
        }
        else
        {
            ChangeInstruction(noMovement);
            ShowInstruction();
        }
    }

    // Function to adjust the statistics

    // Carla: NOT YET TESTED
    private void ScaleStatistics()
    {

        BadCube.transform.localScale = new Vector3(BadCube.transform.localScale.x, BadMovements, BadCube.transform.localScale.z);
        BadCube.transform.localPosition = new Vector3(BadCube.transform.position.x, (float)((BadMovements * 0.5) - 0.5), BadCube.transform.position.z);

        GoodCube.transform.localScale = new Vector3(GoodCube.transform.localScale.x, GoodMovements, GoodCube.transform.localScale.z);
        GoodCube.transform.localPosition = new Vector3(GoodCube.transform.position.x, (float)((GoodMovements * 0.5) - 0.5), GoodCube.transform.position.z);

        Badstatistics.GetComponent<TextMesh>().text = "bad moves: " + BadMovements.ToString();
        Goodstatistics.GetComponent<TextMesh>().text = "good moves: " + GoodMovements.ToString();

     }

    //Function to provide auditory Feedback.

    private void AuditoryFeedback(AudioClip audioClip, float volumeScale)
    {
        AudioSource source;
        source = GetComponent<AudioSource>();
        source.PlayOneShot(audioClip, volumeScale);
    }


    //creating the task for the user
    private void CreateTask()
    {
        Debug.Log("Creating task!");
        HideInstruction();
        Statistics.SetActive(false);
        Animations.SetActive(true);
        sketchAnimation.SetActive(true);
        
        Animation taskAnim = sketchAnimation.GetComponent<Animation>();
        taskAnim.Play();
             
    }


    //different thresholds, different volume scale played on the Hololens
    private void ThresholdsCalculation(ulong id, Dictionary<ulong, Vector3[]> bodies_pos, TextMesh infoText)
    {
        
        spineShoulder = bodies_pos[id][(int)20];
        spineBase = bodies_pos[id][(int)0];
        ActualAngle = (int) AngleBetweenTwoVectors(backstraight, spineBase - spineShoulder);
        KneeAngle = KneeAngleCalculation(id, bodies_pos);
   
        //FeedbackProvided is set to true, and badMoveCounter will be increased only once
        // feedback for a 'small' bad movement according to stages
        if (ActualAngle > 20
            && ActualAngle <= 60
            && KneeAngle > 120
            && FeedbackProvided == 0)
        {
            switch (LearningStage)
            {
                //
                case 0:
                    CustomMessages2.Instance.SendThresholdData(id, 0.2);
                    FeedbackProvided = 1;
                    break;

                case 1:
                    CustomMessages2.Instance.SendThresholdData(id, 0.2);
                    FeedbackProvided = 1;
                    break;

                case 2:
                    FeedbackProvided = 1;
                    break;

                default:
                    break;
            }

        }

        
        //FeedbackProvided is set to true, and badMoveCounter will be increased only once
        // feedback for worst movement
        else if (
            ActualAngle > 60
            && KneeAngle > 120
            && FeedbackProvided == 1)
        {
            switch (LearningStage)
            {
                // stage 0: Auditory, haptic and visual feedback
                case 0:
                    AuditoryFeedback(backSound, 0.5F);
                    CustomMessages2.Instance.SendThresholdData(id, 0.5);
                    FeedbackProvided = 2;
                    BadMoveCounter++;
                    break;

                // stage 1: visual and haptic feedback
                case 1:
                    CustomMessages2.Instance.SendThresholdData(id, 0.5);
                    FeedbackProvided = 2;
                    BadMoveCounter++;
                    break;

                // stage 2: only visual
                case 2:
                    FeedbackProvided = 2;
                    BadMoveCounter++;
                    break;

                default:
                    break;
            }

        }
 

        //FeedbackProvided set to false when person is standing straight
        // Person is standing straight: if the last movement was a good movement he gets positive feedback
        else if (
          ActualAngle <= 50
          && KneeAngle > 130 
          && FeedbackProvided != 0)
        {
            standingCounter++;
            if (FeedbackProvided == -1)
            {
                AuditoryFeedback(positiveSound, 0.5F);
            }
            FeedbackProvided = 0;
        }
        //feedbackProvided set to true when person bends in good movement, and in next frame GoodMoveCounter wont increase
        else if (ActualAngle <= 30
            && KneeAngle < 50
            && FeedbackProvided == 0)
        {
            FeedbackProvided = -1;
            GoodMoveCounter++;
            
        }
    }


    // function that is called when we say Finish, by voice recognition
    public void TaskFinished()
    {
        sketchAnimation.SetActive(false);
        Animations.SetActive(false);
        TaskDone = true;
        TaskAktive = false;
        BadMovements = BadMovements + BadMoveCounter;
        GoodMovements = GoodMovements + GoodMoveCounter;
        TaskCounter++;
        VisualSummativeFeedback();

        

    }

    // Function to start the task again
    public void TaskAgain()
    {
        BadMoveCounter = 0;
        GoodMoveCounter = 0;
        Statistics.SetActive(false);
        klimentBends.SetActive(false);
        Animations.SetActive(false);
        HideInstruction();
        GeneralCounter = 0;
        TaskDone = false;
    }

    public void showAnimation()
    {
        HideInstruction();
        Animations.SetActive(true);
        klimentBends.SetActive(true);
        // the Animation of the guiding gif is shown
        Animation beckAnim = klimentBends.GetComponent<Animation>();
        beckAnim.Play();
    }


    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 3, joint.Position.Y * 3, joint.Position.Z * 3);
    }

    private static Quaternion GetQuaternionFromJointOrientation(Kinect.JointOrientation jointOrientation)
    {
        return new Quaternion(jointOrientation.Orientation.X, jointOrientation.Orientation.Y, jointOrientation.Orientation.Z, jointOrientation.Orientation.W);
    }


    // function to calculate the angle of the knees in order to see if the user bends down with straight legs or not
    private float KneeAngleCalculation(ulong id, Dictionary<ulong, Vector3[]> bodies_pos)
    {
        hipLeft = bodies_pos[id][(int)12];
        kneeLeft = bodies_pos[id][(int)13];
        ankleLeft = bodies_pos[id][(int)14];
        hipRight = bodies_pos[id][(int)16];
        kneeRight = bodies_pos[id][(int)17];
        ankleRight = bodies_pos[id][(int)18];

        KneeAngleLeft =(float)AngleBetweenTwoVectors(kneeLeft - hipLeft, kneeLeft - ankleLeft);
        KneeAngleRight = (float)AngleBetweenTwoVectors(kneeRight - hipRight, kneeRight - ankleRight);



        return (KneeAngleLeft + KneeAngleRight) / 2;
    }
}


