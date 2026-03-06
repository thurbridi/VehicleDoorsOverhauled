using System;
using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  public enum DoorSide { Left, Right }

  public class VehicleDoor : MonoBehaviour
  {
    public class Config
    {
      public float playerOpenTorque = 80f;
      public float playerCloseTorque = -80f;
      public float doorCheckBreakTorque = 80f;
      public float staticFrictionTorque = 5f;
      public float dynamicFrictionTorque = 3f;
      public float breakSpeedDeg = 5f;
      public float captureSpeedDeg = 1f;
      public GameObject door = null;
      public Rigidbody vehicleRigidbody = null;
      public JointLimits openHingeLimits, closedHingeLimits = new JointLimits() { min = 0f, max = 0f };
      public Action onDoorOpened = null;
      public Action onDoorClosed = null;
      public Func<float, bool> isDoorNearClosedPredicate = null;
      public Func<float, bool> isPastDoorcheckAnglePredicate = null;
      public Func<float, bool> isDoorFastEnoughToClosePredicate = null;
      public Axis hingeAxis = Axis.Z;
      public Axis doorAngleAxis = Axis.Y;
      public Axis angularVelocityAxis = Axis.Y;
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
    private bool isColliderHit = false;
    private bool wasColliderHit = false;
    private bool isMotorSlipping = false;


    public void Initialize(Config config)
    {
      this.config = config ?? throw new ArgumentNullException("config");

      gameObject.layer = LayerMask.NameToLayer("HingedObjects");

      Vector3 hingeAxisVec = Vector3.zero;
      hingeAxisVec[(int)config.hingeAxis] = 1f;

      openTorqueVec = hingeAxisVec.normalized * config.playerOpenTorque;
      closeTorqueVec = hingeAxisVec.normalized * config.playerCloseTorque;

      doorRigidbody = this.config.door.GetComponent<Rigidbody>()
        ?? throw new ArgumentException("config.door GameObject must have a Rigidbody component.");
      doorHingeJoint = this.config.door.GetComponent<HingeJoint>()
        ?? throw new ArgumentException("config.door GameObject must have a HingeJoint component.");
      doorMeshCollider = gameObject.GetComponent<Collider>()
        ?? throw new ArgumentException("VehicleDoor must be attached to a GameObject with a Collider.");

      doorHingeJoint.useMotor = true;
      doorHingeJoint.useSpring = false; // Unity recommends not using both motor and spring on the same joint, so we disable spring just in case

      guiUse = FsmVariables.GlobalVariables.GetFsmBool("GUIuse");

      isInitialized = true;
      enabled = true;
    }

    private void ApplyMotorFriction(float doorAngularVelocity)
    {
      float hingeSpeed = Mathf.Abs(doorAngularVelocity);
      isMotorSlipping = UpdateSlipState(isMotorSlipping, hingeSpeed, config.breakSpeedDeg, config.captureSpeedDeg);

      JointMotor motor = doorHingeJoint.motor;
      motor.targetVelocity = 0f;
      motor.force = isMotorSlipping ? config.dynamicFrictionTorque : config.staticFrictionTorque;
      motor.freeSpin = false;
      doorHingeJoint.motor = motor;
      doorHingeJoint.useMotor = true;
    }

    private static bool UpdateSlipState(bool isSlipping, float speedDeg, float breakSpeedDeg, float captureSpeedDeg)
    {
      if (!isSlipping && speedDeg > breakSpeedDeg)
        return true;
      if (isSlipping && speedDeg < captureSpeedDeg)
        return false;
      return isSlipping;
    }

    private float GetVectorComponent(Vector3 vec, Axis axis)
    {
      if (!Enum.IsDefined(typeof(Axis), axis))
        throw new ArgumentOutOfRangeException(nameof(axis), axis, null);

      return vec[(int)axis];
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
      isColliderHit = UnifiedRaycast.GetHitInteraction(doorMeshCollider);

      if (isColliderHit)
      {
        guiUse.Value = true;
        wasColliderHit = true;

        if (Input.GetMouseButtonDown(0))
        {
          playerIntent = PlayerIntent.Open;
        }
        else if (Input.GetMouseButtonDown(1))
        {
          playerIntent = PlayerIntent.Close;
        }
      }
      else if (wasColliderHit)
      {
        guiUse.Value = false;
        wasColliderHit = false;
      }

      if ((playerIntent == PlayerIntent.Open && !Input.GetMouseButton(0)) ||
          (playerIntent == PlayerIntent.Close && !Input.GetMouseButton(1)))
      {
        playerIntent = PlayerIntent.None;
      }
    }

    void FixedUpdate()
    {
      float doorAngularVelocity = GetVectorComponent(doorRigidbody.angularVelocity, config.angularVelocityAxis);
      ApplyMotorFriction(doorAngularVelocity);

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