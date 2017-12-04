using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnetLLAPISample;
using UnityEngine;
using Kinect = Windows.Kinect;

[Serializable]
public struct SimpleBody
{
	public List<SimpleJoint> Joints;
}

[Serializable]
public struct SimpleJoint
{
	public Vector3 Position;
	public int TrackingState;

	// Orientation
//	public float X;
//	public float Y;
//	public float Z;
//	public float W;
	public int X;
	public int Y;
	public int Z;
	public int W;
}

public class BodySender : MonoBehaviour {
	public LLAPINetworkManager NetworkManager;
	public GameObject BodySourceManager;

	private BodySourceManager _BodyManager;

	void Update()
	{
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
		// 一人目のTrackedの人が見つかるまで
		SimpleBody simpleBody1 = new SimpleBody();	// Position, TrackingState
		SimpleBody simpleBody2 = new SimpleBody();	// Orientation
		foreach (var body in data) {
			simpleBody1 = GenerateSimpleBody1(body);
			simpleBody2 = GenerateSimpleBody2(body);
			if (body.IsTracked)
			{
				break;
			}
		}

		byte[] sendData1;
		byte[] sendData2;
		string serialisedMsg1 = JsonUtility.ToJson(simpleBody1);
		string serialisedMsg2 = JsonUtility.ToJson(simpleBody2);
		Debug.Log(serialisedMsg1 + serialisedMsg2);

		using (var memoryStream = new MemoryStream())
		{
			using (var gzipStream = new Unity.IO.Compression.GZipStream(memoryStream, Unity.IO.Compression.CompressionMode.Compress))
			using (var writer = new StreamWriter(gzipStream))
			{
				writer.Write(serialisedMsg1);
			}
			sendData1 = memoryStream.ToArray();
		}
        NetworkManager.SendPacketData(sendData1, UnityEngine.Networking.QosType.Unreliable);
		using (var memoryStream = new MemoryStream())
		{
			using (var gzipStream = new Unity.IO.Compression.GZipStream(memoryStream, Unity.IO.Compression.CompressionMode.Compress))
			using (var writer = new StreamWriter(gzipStream))
			{
				writer.Write(serialisedMsg2);
			}
			sendData2 = memoryStream.ToArray();
		}

		
		NetworkManager.SendPacketData(sendData2, UnityEngine.Networking.QosType.Unreliable);
		Debug.Log("sender update()");
	}

	SimpleBody GenerateSimpleBody1(Kinect.Body body)
	{
		SimpleBody simpleBody;
		var joints = new List<SimpleJoint>();

		for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.HipLeft; jt++)
		{
			Kinect.Joint joint = body.Joints [jt];
			SimpleJoint sendJoint;
			if (jt != Kinect.JointType.Neck && jt != Kinect.JointType.Head) {
				sendJoint.Position = GetVector3FromJoint (joint);
				sendJoint.TrackingState = (int)joint.TrackingState;
				//			sendJoint.X = body.JointOrientations[jt].Orientation.X;
				//			sendJoint.Y = body.JointOrientations[jt].Orientation.Y;
				//			sendJoint.Z = body.JointOrientations[jt].Orientation.Z;
				//			sendJoint.W = body.JointOrientations[jt].Orientation.W;
				sendJoint.X = (int)(body.JointOrientations[jt].Orientation.X * 10000);
				sendJoint.Y = (int)(body.JointOrientations[jt].Orientation.Y * 10000);
				sendJoint.Z = (int)(body.JointOrientations[jt].Orientation.Z * 10000);
				sendJoint.W = (int)(body.JointOrientations[jt].Orientation.W * 10000);
			}
			else
			{
				sendJoint.Position = Vector3.zero;
				sendJoint.TrackingState = 0;
//				sendJoint.X = 0.0f;
//				sendJoint.Y = 0.0f;
//				sendJoint.Z = 0.0f;
//				sendJoint.W = 0.0f;
				sendJoint.X = 0;
				sendJoint.Y = 0;
				sendJoint.Z = 0;
				sendJoint.W = 0;
			}
			joints.Add (sendJoint);
		}
		simpleBody.Joints = joints;
		return simpleBody;
	}

	SimpleBody GenerateSimpleBody2(Kinect.Body body)
	{
		SimpleBody simpleBody;
		var joints = new List<SimpleJoint>();

		for (Kinect.JointType jt = Kinect.JointType.KneeLeft; jt <= Kinect.JointType.ThumbRight; jt++)
		{
			Kinect.Joint joint = body.Joints[jt];
			SimpleJoint sendJoint;
			sendJoint.Position = GetVector3FromJoint(joint);
			sendJoint.TrackingState = (int)joint.TrackingState;
//			sendJoint.X = body.JointOrientations[jt].Orientation.X;
//			sendJoint.Y = body.JointOrientations[jt].Orientation.Y;
//			sendJoint.Z = body.JointOrientations[jt].Orientation.Z;
//			sendJoint.W = body.JointOrientations[jt].Orientation.W;
			sendJoint.X = (int)(body.JointOrientations[jt].Orientation.X * 10000);
			sendJoint.Y = (int)(body.JointOrientations[jt].Orientation.Y * 10000);
			sendJoint.Z = (int)(body.JointOrientations[jt].Orientation.Z * 10000);
			sendJoint.W = (int)(body.JointOrientations[jt].Orientation.W * 10000);

			joints.Add(sendJoint);
		}
		simpleBody.Joints = joints;
		return simpleBody;
	}

	private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
	{
		return new Vector3(joint.Position.X, joint.Position.Y, -joint.Position.Z);
	}
}