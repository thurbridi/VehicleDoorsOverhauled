using System;
using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsReworked
{
  public class VehicleDoor : MonoBehaviour
  {
    public class Config
    {
      public Axis hingeAxis;
      public float playerOpenTorque = 80f;
      public float playerCloseTorque = -80f;
      public float doorCheckBreakTorque = 80f;
      public GameObject door = null;
      public Rigidbody vehicleRigidbody = null;
      public JointLimits openHingeLimits, closedHingeLimits;
      public Action onDoorOpened = null;
      public Action onDoorClosed = null;
      public Func<float, bool> isDoorNearClosedPredicate = null;
      public Func<float, bool> isPastDoorcheckAnglePredicate = null;
      public Func<float, bool> isDoorFastEnoughToClosePredicate = null;
      public Axis doorAngleAxis;
      public Axis angularVelocityAxis;
    }

    public enum Axis { X, Y, Z }

    private enum PlayerIntent { None, Open, Close }

    public Config config = null;

    private bool isInitialized = false;
    private float currentDoorAngle;
    private bool isDoorOpen = false;
    private Vector3 openTorqueVec, closeTorqueVec;
    private Collider doorMeshCollider;
    private Rigidbody doorRigidbody;
    private HingeJoint doorHingeJoint;
    private FixedJoint doorCheck;
    private FsmBool guiUse;
    private PlayerIntent playerIntent = PlayerIntent.None;
    private bool isRaycastOverCollider = false;
    private bool wasRaycastOverCollider = false;


    public void Initialize(Config config)
    {
      this.config = config ?? throw new ArgumentNullException("config");

      Vector3 hingeAxisVec;
      switch (config.hingeAxis)
      {
        case Axis.X:
          hingeAxisVec = Vector3.right;
          break;
        case Axis.Y:
          hingeAxisVec = Vector3.up;
          break;
        case Axis.Z:
          hingeAxisVec = Vector3.forward;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(config.hingeAxis), config.hingeAxis, null);
      }

      openTorqueVec = hingeAxisVec.normalized * config.playerOpenTorque;
      closeTorqueVec = hingeAxisVec.normalized * config.playerCloseTorque;

      doorRigidbody = this.config.door.GetComponent<Rigidbody>()
        ?? throw new ArgumentException("config.door GameObject must have a Rigidbody component.");
      doorHingeJoint = this.config.door.GetComponent<HingeJoint>()
        ?? throw new ArgumentException("config.door GameObject must have a HingeJoint component.");
      doorMeshCollider = gameObject.GetComponent<Collider>()
        ?? throw new ArgumentException("VehicleDoor must be attached to a GameObject with a Collider.");

      guiUse = FsmVariables.GlobalVariables.GetFsmBool("GUIuse");

      isInitialized = true;
      enabled = true;
    }

    private float GetVectorComponent(Vector3 vec, Axis axis)
    {
      switch (axis)
      {
        case Axis.X:
          return vec.x;
        case Axis.Y:
          return vec.y;
        case Axis.Z:
          return vec.z;
        default:
          throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
      }
    }

    void OnDoorOpened()
    {
      config.onDoorOpened?.Invoke();

      isDoorOpen = true;
      doorHingeJoint.limits = config.openHingeLimits;

      // Rivett specific
      var fixedJoint = gameObject.GetComponent<FixedJoint>();
      if (fixedJoint != null) Destroy(fixedJoint);
    }

    void OnDoorClosed()
    {
      config.onDoorClosed?.Invoke();

      isDoorOpen = false;

      doorHingeJoint.limits = config.closedHingeLimits;
    }

    void Awake()
    {
      if (!isInitialized)
      {
        enabled = false;
      }
    }

    void Update()
    {
      isRaycastOverCollider = UnifiedRaycast.GetHitAll(doorMeshCollider);
      playerIntent = PlayerIntent.None;

      if (isRaycastOverCollider)
      {
        guiUse.Value = true;
        wasRaycastOverCollider = true;

        if (Input.GetMouseButton(0))
        {
          playerIntent = PlayerIntent.Open;
        }
        else if (Input.GetMouseButton(1))
        {
          playerIntent = PlayerIntent.Close;
        }
      }
      else if (wasRaycastOverCollider)
      {
        guiUse.Value = false;
        wasRaycastOverCollider = false;
      }
    }

    void FixedUpdate()
    {
      switch (playerIntent)
      {
        case PlayerIntent.None:
          // Door is closed and no input, early return.
          if (!isDoorOpen)
            return;
          break;
        case PlayerIntent.Open:
          if (!isDoorOpen) OnDoorOpened();
          doorRigidbody.AddRelativeTorque(openTorqueVec);
          break;
        case PlayerIntent.Close:
          doorRigidbody.AddRelativeTorque(closeTorqueVec);
          if (doorCheck != null) Destroy(doorCheck);
          break;
      }

      currentDoorAngle = GetVectorComponent(config.door.transform.localEulerAngles, config.doorAngleAxis);

      // Door check
      bool isPastDoorCheckAngle = config.isPastDoorcheckAnglePredicate(currentDoorAngle);

      if (isPastDoorCheckAngle && (playerIntent != PlayerIntent.Close) && doorCheck == null)
      {
        doorCheck = config.door.AddComponent<FixedJoint>();
        doorCheck.connectedBody = config.vehicleRigidbody;
        doorCheck.breakTorque = config.doorCheckBreakTorque;
        return; // Early return because door check angle should not be close to closing the door
      }

      float doorAngularVelocity = GetVectorComponent(doorRigidbody.angularVelocity, config.angularVelocityAxis);

      // Close door
      bool isDoorNearClosed = config.isDoorNearClosedPredicate(currentDoorAngle);
      bool isDoorFastEnoughToClose = config.isDoorFastEnoughToClosePredicate(doorAngularVelocity);
      if (isDoorOpen && isDoorNearClosed && isDoorFastEnoughToClose)
      {
        OnDoorClosed();
      }
    }
  }
}