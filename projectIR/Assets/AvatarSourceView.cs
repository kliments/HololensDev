using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using System.IO;
using System;
using HoloToolkit.Unity;
using HoloToolkit.Sharing;
//using LightBuzz.Vitruvius;

public class AvatarSourceView : MonoBehaviour
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;
    public double minZ;
    public double maxZ;
    public Material highlight;
    public Boolean FeedbackProvided = false;
    public AudioClip hapticSound;


    [Header("SoundClips")]
    public AudioClip backSound;


    //private HoloUser User;
    Vector3 spineShoulder;
    Vector3 spineBase;
    Vector3 hipRight;
    Vector3 kneeRight;
    Vector3 ankleRight;
    Vector3 hipLeft;
    Vector3 kneeLeft;
    Vector3 ankleLeft;
    private int ActualAngle;
    private float KneeAngleLeft;
    private float KneeAngleRight;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    public string avatarAssetName;

    // Public function so other scripts can send out the data
    public Dictionary<ulong, GameObject> GetData()
    {
        return _Bodies;
    }

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
   void start() {

    }
    void Update()
    {
        Debug.Log(BodyView.UserID + "and" + BodyView.backstraight);
        if (BodySourceManager == null)
        {
            return;
        }

        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }

        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                trackedIds.Add(body.TrackingId);
            }
        }

        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }


                if (body.IsTracked)
                {
                    if (!_Bodies.ContainsKey(body.TrackingId))
                    {
                        _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                    }
                             
                        RefreshBodyObject(body, _Bodies[body.TrackingId]);
                        SetAvatarScale(_Bodies[body.TrackingId]);

                           //ThresholdsCalculation(BodyView.UserID, body, GetComponent<TextMesh>());
                         
                }
            }
        

    }


    //is supposed to calculate the distance


    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = new GameObject();
            //GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;

        }
        // Add avatar gameobject from source
        GameObject avatar = Instantiate(Resources.Load(avatarAssetName, typeof(GameObject))) as GameObject;
        avatar.transform.parent = body.transform;
        avatar.name = "Avatar";
        //Try to add tags for different bodyparts
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
        //avatar.transform.parent = body.transform.Find("SpineBase");
        return body;
    }

    private void SetAvatarScale(GameObject bodyObject)
    {

        Transform avatar = bodyObject.transform.FindChild("Avatar");
        if (avatar.localScale.x != 1) {
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

    }



    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        Transform avatar = bodyObject.transform.FindChild("Avatar");
        //StreamWriter sw = new StreamWriter(@"C:\Users\Carla\workspace\Ertrag\KinectDataTX.txt", true);

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.JointOrientation sourceJointOrientation = body.JointOrientations[jt];


            if (_BoneMap.ContainsKey(jt))
            {

                if (_RigMap.ContainsKey(jt.ToString())) {
                    Transform avatarItem = avatar.FindChild(_RigMap[jt.ToString()]);
                    Transform bodyItem = bodyObject.transform.FindChild(jt.ToString());

                    //our Code: start value 0.0, so if it's not the start value and checkBack returned true
                    //send audio signal, SubPac as output medium, so starts vibrating
                    /*if (bodyObject.transform.FindChild("SpineShoulder").position.y != 0.0 &&
                        FeedbackProvided == false &&
                        checkBack(GetVector3FromJoint(body.Joints[Kinect.JointType.SpineShoulder]),
                                    GetVector3FromJoint(body.Joints[Kinect.JointType.SpineBase]),
                                     GetVector3FromJoint(body.Joints[Kinect.JointType.FootLeft]),
                                     120.0) == true)
                    {
                        FeedbackProvided = true;
                        //HapticFeedback();
                        VisualFeedBack();
                    }
                    else if (checkBack(GetVector3FromJoint(body.Joints[Kinect.JointType.SpineShoulder]),
                                    GetVector3FromJoint(body.Joints[Kinect.JointType.SpineBase]),
                                     GetVector3FromJoint(body.Joints[Kinect.JointType.FootLeft]),
                                     120.0) == false)
                    {
                        FeedbackProvided = false;
                    }*/

                    if (jt.ToString() == "SpineBase")
                    {
                        avatarItem.position = bodyItem.position;

                    }
                    avatarItem.rotation = bodyItem.rotation * _RigMapOffsets[jt.ToString()];
                }
            }
            ;
            Transform jointObj = bodyObject.transform.FindChild(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            jointObj.localRotation = GetQuaternionFromJointOrientation(sourceJointOrientation);

        }
        //sw.Close();
    }



    //Helperfunction to calculate the Angle between two Vectors
    private static double AngleBetweenTwoVectors(Vector3 vectorA, Vector3 vectorB)
    {
        double dotProduct;
        vectorA.Normalize();
        vectorB.Normalize();
        dotProduct = Vector3.Dot(vectorA, vectorB);

        return (double)Math.Acos(dotProduct) / Math.PI * 180;
    }

    //Function that provides Haptic Feedback
    void HapticFeedback(NetworkInMessage msg) {

        Debug.Log("Get Threshold Data");
        msg.ReadInt64();
        double volumeScale = msg.ReadDouble();
        Debug.Log("Subpack should start vibrating");
        AudioSource source;
        source = GetComponent<AudioSource>();
        source.PlayOneShot(hapticSound, (float)volumeScale);
    }


    //Function that provides the visual Feedback
    private void VisualFeedBack()
    {
        Material[] highlights = new Material[] {highlight};
        GameObject[] bodyparts = GameObject.FindGameObjectsWithTag("Back");
        foreach (GameObject bodypart in bodyparts)
        {
          bodypart.GetComponent<SkinnedMeshRenderer>().materials = highlights;
          Debug.Log(bodypart + "has been highlighted");
        }
    }

    //Function that sends an audio signal to the subpack, starts vibrating
    

    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }

    private void AuditoryFeedback(float volumeScale)
    {
        Debug.Log("plays something");
        AudioSource source;
        source = GetComponent<AudioSource>();
        source.PlayOneShot(backSound, volumeScale);
    }

    //different thresholds, different volume scale played on the Subpack
    private void ThresholdsCalculation(ulong id, Kinect.Body body, TextMesh infoText)
    {
        Debug.Log("Threshold Calculation");
        spineShoulder = GetVector3FromJoint(body.Joints[Kinect.JointType.SpineShoulder]);
        spineBase = GetVector3FromJoint(body.Joints[Kinect.JointType.SpineBase]);
        ActualAngle = (int)AngleBetweenTwoVectors(BodyView.backstraight, spineBase - spineShoulder);
        Boolean KneeAngle = KneeAngleCalculation(id, body);

        Debug.Log(spineShoulder + " and" + spineBase);
        //FeedbackProvided is set to true
        if (ActualAngle > 60
            && !KneeAngle
            && !FeedbackProvided)
        {

            AuditoryFeedback(1F);
            FeedbackProvided = true;
        }

        //FeedbackProvided set to false when person is standing straight
        else if (
          ActualAngle <= 60
          && KneeAngle == false
          && FeedbackProvided == true)
        {
            FeedbackProvided = false;
        }
        //feedbackProvided set to true when person bends in good movement, and in next frame GoodMoveCounter wont increase
        else if (ActualAngle <= 60
            && KneeAngle == true
            && !FeedbackProvided)
        {
            AuditoryFeedback(0.5F);
            FeedbackProvided = true;

        }
    }

    //Knee angle calculation
    private Boolean KneeAngleCalculation(ulong id, Kinect.Body body)
    {
        hipLeft = GetVector3FromJoint(body.Joints[Kinect.JointType.HipLeft]);
        kneeLeft = GetVector3FromJoint(body.Joints[Kinect.JointType.KneeLeft]);
        ankleLeft = GetVector3FromJoint(body.Joints[Kinect.JointType.AnkleLeft]);
        hipRight = GetVector3FromJoint(body.Joints[Kinect.JointType.HipRight]);
        kneeRight = GetVector3FromJoint(body.Joints[Kinect.JointType.KneeRight]);
        ankleRight = GetVector3FromJoint(body.Joints[Kinect.JointType.AnkleRight]);

        KneeAngleLeft = (float)AngleBetweenTwoVectors(kneeLeft - hipLeft, kneeLeft - ankleLeft);
        KneeAngleRight = (float)AngleBetweenTwoVectors(kneeRight - hipRight, kneeRight - ankleRight);

        if (KneeAngleLeft < 40
            && KneeAngleRight < 40)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 3, joint.Position.Y * 3 , joint.Position.Z * 3);
    }

    private static Quaternion GetQuaternionFromJointOrientation(Kinect.JointOrientation jointOrientation)
    {
        return new Quaternion(jointOrientation.Orientation.X, jointOrientation.Orientation.Y, jointOrientation.Orientation.Z, jointOrientation.Orientation.W);
    }
}
