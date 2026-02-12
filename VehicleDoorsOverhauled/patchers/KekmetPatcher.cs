using UnityEngine;

namespace VehicleDoorsOverhauled
{
  static class KekmetPatcher
  {
    static Transform doors;
    static Rigidbody vehicleRigidbody;
    private const float playerInteractionTorque = 50f;
    private const float doorCheckBreakTorque = 75f;
    private const float angularVelocityToCloseDoor = 2.2f;
    const string audioGroup = "CarFoley";
    const string audioClipOpen = "car_old_door_open";
    const string audioClipClose = "car_old_door_close";

    public static void Patch()
    {
      Initialize();

      PatchLeftDoor();
      PatchRightDoor();
    }

    static void Initialize()
    {
      Transform vehicle = GameObject.Find("KEKMET(350-400psi)").transform;
      vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
      doors = vehicle.Find("DriverDoors");
    }

    static void PatchLeftDoor()
    {
      Transform door = doors.transform.Find("doorl");


      var useDoorFsm = door.GetComponent<PlayMakerFSM>();
      useDoorFsm.enabled = false;

      var doorComponent = door.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(new VehicleDoor.Config()
      {
        playerOpenTorque = playerInteractionTorque,
        playerCloseTorque = -playerInteractionTorque,
        doorCheckBreakTorque = doorCheckBreakTorque,
        hingeAxis = VehicleDoor.Axis.Z,
        door = door.gameObject,
        openHingeLimits = new JointLimits() { min = 0.25f, max = 80f },
        closedHingeLimits = new JointLimits() { min = 0f, max = 0f },
        vehicleRigidbody = vehicleRigidbody,
        onDoorOpened = () => OnDoorOpened(door.transform),
        onDoorClosed = () => OnDoorClosed(door.transform),
        isDoorNearClosedPredicate = (doorAngle) => doorAngle <= 275f,
        isPastDoorcheckAnglePredicate = (doorAngle) => doorAngle > 350f,
        isDoorFastEnoughToClosePredicate = (doorAngularVelocity) => doorAngularVelocity <= -angularVelocityToCloseDoor,
        angularVelocityAxis = VehicleDoor.Axis.Y,
        doorAngleAxis = VehicleDoor.Axis.Y,
      });
    }

    static void PatchRightDoor()
    {
      Transform door = doors.transform.Find("doorr");

      var useDoorFsm = door.GetComponent<PlayMakerFSM>();
      useDoorFsm.enabled = false;

      var doorComponent = door.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(new VehicleDoor.Config()
      {
        playerOpenTorque = -playerInteractionTorque,
        playerCloseTorque = playerInteractionTorque,
        doorCheckBreakTorque = doorCheckBreakTorque,
        hingeAxis = VehicleDoor.Axis.Z,
        door = door.gameObject,
        openHingeLimits = new JointLimits() { min = -80f, max = -0.25f },
        closedHingeLimits = new JointLimits() { min = 0f, max = 0f },
        vehicleRigidbody = vehicleRigidbody,
        onDoorOpened = () => OnDoorOpened(door.transform),
        onDoorClosed = () => OnDoorClosed(door.transform),
        isDoorNearClosedPredicate = (doorAngle) => doorAngle >= 265f,
        isPastDoorcheckAnglePredicate = (doorAngle) => doorAngle < 190f,
        isDoorFastEnoughToClosePredicate = (doorAngularVelocity) => doorAngularVelocity >= angularVelocityToCloseDoor,
        angularVelocityAxis = VehicleDoor.Axis.Y,
        doorAngleAxis = VehicleDoor.Axis.Y,
      });
    }

    static void OnDoorOpened(Transform audioSource)
    {
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: audioSource, variationName: audioClipOpen);
    }

    static void OnDoorClosed(Transform audioSource)
    {
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: audioSource, variationName: audioClipClose);
    }
  }
}
