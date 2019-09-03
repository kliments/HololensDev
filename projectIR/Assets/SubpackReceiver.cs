/*
 * BodyDataReceiver.cs
 *
 * Receives body data from the network
 * Requires CustomMessages2.cs
 */

using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Kinect = Windows.Kinect;

// Receives the body data messages
public class SubpackReceiver : Singleton<SubpackReceiver>
{
    public AudioClip hapticSound;
    

    void Start()
    {
        CustomMessages2.Instance.MessageHandlers[CustomMessages2.TestMessageID.BodyData] =
            this.HapticFeedback;
    }

    // Called when reading in Kinect body data
    void HapticFeedback(NetworkInMessage msg)
    {
        
        Debug.Log("Get Threshold Data");
        msg.ReadInt64();
        double volumeScale = msg.ReadDouble();
        Debug.Log("Subpack should start vibrating");
        AudioSource source;
        source = GetComponent<AudioSource>();
        source.PlayOneShot(hapticSound, (float)volumeScale);
        // Parse the message
        }
}
