using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XRInputController : MonoBehaviour
{
    private GameObject palmObj;
    private static readonly HandFinger[] handFingers = System.Enum.GetValues(typeof(HandFinger)) as HandFinger[];

    private readonly Dictionary<HandFinger, GameObject[]> handFingerGameObjects = new Dictionary<HandFinger, GameObject[]>();

    private readonly List<Bone> fingerBones = new List<Bone>();
    private InputDeviceCharacteristics handCharacteristics => gameObject.name.Contains("Left") ? InputDeviceCharacteristics.Left : InputDeviceCharacteristics.Right;
    
    private void Update()
    {
        GetXRJoints();
    }
    
    private void GetXRJoints()
    {
        //Get XR device with hand tracking support, left or right hand based on a flag
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HandTracking | handCharacteristics, devices);
        Hand hand = default;
        foreach (var device in devices)
        {
            if (device.TryGetFeatureValue(CommonUsages.isTracked, out bool isTracked) && isTracked && device.TryGetFeatureValue(CommonUsages.handData, out hand))
            {
                break;
            }
        }

        if (hand != default)
        {
            UpdateHandBones(hand);
        }
    }

    private void UpdateHandBones(Hand hand)
    {
        //Make palm
        if (hand.TryGetRootBone(out Bone palm))
        {
            if (palmObj == null) palmObj = InstantiateJoint();
            //Move palm to root location
            bool posAvailable = palm.TryGetPosition(out Vector3 pos);
            bool rotAvailable = palm.TryGetRotation(out Quaternion rot);
            if (posAvailable || rotAvailable) palmObj.transform.SetPositionAndRotation(pos, rot);
        }

        foreach (HandFinger finger in handFingers)
        {
            if (hand.TryGetFingerBones(finger, fingerBones))
            {
                //make finger joints if they are not in the dictionary
                if (!handFingerGameObjects.ContainsKey(finger))
                {
                    GameObject[] jointArray = new GameObject[fingerBones.Count];
                    for (int i = 0; i < fingerBones.Count; i++)
                    {
                        jointArray[i] = InstantiateJoint();
                    }

                    handFingerGameObjects[finger] = jointArray;
                }

                GameObject[] fingerJointObjs = handFingerGameObjects[finger];
                
                for (int i = 0; i < fingerBones.Count; i++)
                {
                    Bone bone = fingerBones[i];
                    bool pos = bone.TryGetPosition(out Vector3 position);
                    bool rot = bone.TryGetRotation(out Quaternion rotation);
                    if (pos || rot) fingerJointObjs[i].transform.SetPositionAndRotation(position, rotation);
                }
            }
        }
    }

    /// <summary>
    /// Makes a sphere and parents it to this gameObject.
    /// </summary>
    /// <returns></returns>
    private GameObject InstantiateJoint()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(obj.GetComponent<Collider>());
        obj.transform.localScale = Vector3.one * 0.015f;
        obj.transform.parent = this.transform;
        return obj;
    }
}
