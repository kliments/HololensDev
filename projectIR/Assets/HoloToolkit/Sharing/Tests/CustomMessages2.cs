// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Sharing;
using HoloToolkit.Unity;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Kinect = Windows.Kinect;

public class CustomMessages2 : Singleton<CustomMessages2>
{
    /// <summary>
    /// Message enum containing our information bytes to share.
    /// The first message type has to start with UserMessageIDStart
    /// so as not to conflict with HoloToolkit internal messages.
    /// </summary>   
    public enum TestMessageID : byte
    {
        //HeadTransform = MessageID.UserMessageIDStart
        ThresholdData = MessageID.RoomAnchor,
        BodyData = MessageID.UserMessageIDStart,
        Max

    }

    public enum UserMessageChannels
    {
        Anchors = MessageChannel.UserMessageChannelStart,
    }

    /// <summary>
    /// Cache the local user's ID to use when sending messages
    /// </summary>
    public long localUserID
    {
        get; set;
    }

    public bool GotThreshold { get; private set; }
    public delegate void MessageCallback(NetworkInMessage msg);
    private Dictionary<TestMessageID, MessageCallback> _MessageHandlers = new Dictionary<TestMessageID, MessageCallback>();
    public Dictionary<TestMessageID, MessageCallback> MessageHandlers
    {
        get
        {
            return _MessageHandlers;
        }
    }

    /// <summary>
    /// Helper object that we use to route incoming message callbacks to the member
    /// functions of this class
    /// </summary>
    NetworkConnectionAdapter connectionAdapter;

    /// <summary>
    /// Cache the connection object for the sharing service
    /// </summary>
    NetworkConnection serverConnection;

    void Start()
    {
        SharingStage.Instance.SharingManagerConnected += SharingManagerConnected;


    }

    private void SharingManagerConnected(object sender, System.EventArgs e)
    {
        InitializeMessageHandlers();
    }

    void InitializeMessageHandlers()
    {
        SharingStage sharingStage = SharingStage.Instance;

        if (sharingStage == null)
        {
            Debug.Log("Cannot Initialize CustomMessages. No SharingStage instance found.");
            return;
        }

        serverConnection = sharingStage.Manager.GetServerConnection();
        if (serverConnection == null)
        {
            Debug.Log("Cannot initialize CustomMessages. Cannot get a server connection.");
            return;
        }

        connectionAdapter = new NetworkConnectionAdapter();
        connectionAdapter.MessageReceivedCallback += OnMessageReceived;

        // Cache the local user ID
        this.localUserID = SharingStage.Instance.Manager.GetLocalUser().GetID();

        for (byte index = (byte)TestMessageID.BodyData; index < (byte)TestMessageID.Max; index++)
        {
            if (MessageHandlers.ContainsKey((TestMessageID)index) == false)
            {
                MessageHandlers.Add((TestMessageID)index, null);
            }

            serverConnection.AddListener(index, connectionAdapter);
        }
    }

    private NetworkOutMessage CreateMessage(byte MessageType)
    {
        NetworkOutMessage msg = serverConnection.CreateMessage(MessageType);
        msg.Write(MessageType);
        // Add the local userID so that the remote clients know whose message they are receiving
        // msg.Write(localUserID);
        return msg;
    }

    /*    public void SendHeadTransform(Vector3 position, Quaternion rotation)
        {
            // If we are connected to a session, broadcast our head info
            if (this.serverConnection != null && this.serverConnection.IsConnected())
            {
                // Create an outgoing network message to contain all the info we want to send
                NetworkOutMessage msg = CreateMessage((byte)TestMessageID.HeadTransform);

                AppendTransform(msg, position, rotation);

                // Send the message as a broadcast, which will cause the server to forward it to all other users in the session.
                this.serverConnection.Broadcast(
                    msg,
                    MessagePriority.Immediate,
                    MessageReliability.UnreliableSequenced,
                    MessageChannel.Avatar);
            }
        }
    */
    public void SendSubpackData(float volumeScale)
    {
        // If we are connected to a session, broadcast our info
        Debug.Log("SendBodyData");

        if (this.serverConnection != null && this.serverConnection.IsConnected())
        {
            NetworkOutMessage msg = CreateMessage((byte)TestMessageID.ThresholdData);

            msg.Write(volumeScale);


            // Send the message as a broadcast
            this.serverConnection.Broadcast(
                msg,
                MessagePriority.Immediate,
                MessageReliability.UnreliableSequenced,
                MessageChannel.Avatar);
        }
    }


    public void SendBodyData(ulong trackingID, GameObject bodyData)
    {
        // If we are connected to a session, broadcast our info
        //Debug.Log("SendBodyData");

        if (this.serverConnection != null && this.serverConnection.IsConnected())
        {
            NetworkOutMessage msg = CreateMessage((byte)TestMessageID.BodyData);

            msg.Write((long)trackingID);

            for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
            {
                Transform bodyItem = bodyData.transform.FindChild(jt.ToString());
                AppendTransform(msg, bodyItem);
            }
            // Send the message as a broadcast
            this.serverConnection.Broadcast(
                msg,
                MessagePriority.Immediate,
                MessageReliability.UnreliableSequenced,
                MessageChannel.Avatar);
        }
    }


    //Carla: Test to send data to AvatarSourceView for Haptic feedback
    public void SendThresholdData(ulong trackingID, double thresholdData)
    {
        // If we are connected to a session, broadcast our info
        Debug.Log("SendThresholdata");

        if (this.serverConnection != null && this.serverConnection.IsConnected())
        {
            NetworkOutMessage msg = CreateMessage((byte)TestMessageID.BodyData);

            msg.Write((long)trackingID);
            msg.Write(thresholdData);

            // Send the message as a broadcast
            this.serverConnection.Broadcast(
                msg,
                MessagePriority.Immediate,
                MessageReliability.UnreliableSequenced,
                MessageChannel.Avatar);
        }
    }


    void OnDestroy()
    {

        if (this.serverConnection != null)
        {
            for (byte index = (byte)TestMessageID.BodyData; index < (byte)TestMessageID.Max; index++)
            {
                this.serverConnection.RemoveListener(index, this.connectionAdapter);
            }
            this.connectionAdapter.MessageReceivedCallback -= OnMessageReceived;
        }
    }

    void OnMessageReceived(NetworkConnection connection, NetworkInMessage msg)
    {

        byte messageType = msg.ReadByte();
        MessageCallback messageHandler = MessageHandlers[(TestMessageID)messageType];
        if (messageHandler != null)
        {
            messageHandler(msg);
        }
    }

    #region HelperFunctionsForWriting

    void AppendTransform(NetworkOutMessage msg, Transform transform)
    {
        AppendVector3(msg, transform.position);
        AppendQuaternion(msg, transform.rotation);
    }

    void AppendVector3(NetworkOutMessage msg, Vector3 vector)
    {
        msg.Write(vector.x);
        msg.Write(vector.y);
        msg.Write(vector.z);
    }

    void AppendQuaternion(NetworkOutMessage msg, Quaternion rotation)
    {
        msg.Write(rotation.x);
        msg.Write(rotation.y);
        msg.Write(rotation.z);
        msg.Write(rotation.w);
    }

    #endregion HelperFunctionsForWriting

    #region HelperFunctionsForReading

    public Vector3 ReadVector3(NetworkInMessage msg)
    {
        return new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
    }

    public Quaternion ReadQuaternion(NetworkInMessage msg)
    {
        return new Quaternion(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
    }

    #endregion HelperFunctionsForReading
}