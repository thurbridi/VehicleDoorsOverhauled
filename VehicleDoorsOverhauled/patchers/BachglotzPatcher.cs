using MSCLoader;
using UnityEngine;

namespace VehicleDoorsOverhauled
{
  static class BachglotzPatcher
  {
    static Transform doors;
    static Rigidbody vehicleRigidbody;
    static PlayMakerFSM interiorLightFsm;
    private const float playerInteractionTorque = 50f;
    private const float doorCheckBreakTorque = 75f;
    private const float angularVelocityToCloseDoor = 2.2f;
    const string audioGroup = "CarFoley";
    const string audioClipOpen = "bach_door_open";
    const string audioClipClose = "bach_door_close";

    public static void Patch()
    {
      Initialize();
      PatchLeftDoor();
      PatchRightDoor();
    }

    static void Initialize()
    {
      Transform vehicle = GameObject.Find("BACHGLOTZ(1905kg)").transform;
      vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
      doors = vehicle.Find("DriverDoors");
      interiorLightFsm = vehicle.Find("LOD/InteriorLight/Use").GetComponent<PlayMakerFSM>();
    }

    static void PatchLeftDoor()
    {
      Transform door = doors.transform.Find("door(leftx)");
      Transform doorHandle = door.Find("doors/Handle");

      var useDoorFsm = doorHandle.GetComponent<PlayMakerFSM>();
      useDoorFsm.enabled = false;

      var doorComponent = doorHandle.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(new VehicleDoor.Config()
      {
        playerOpenTorque = playerInteractionTorque,
        playerCloseTorque = -playerInteractionTorque,
        doorCheckBreakTorque = doorCheckBreakTorque,
        hingeAxis = VehicleDoor.Axis.Z,
        door = door.gameObject,
        openHingeLimits = new JointLimits() { min = 0f, max = 80f },
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
      Transform door = doors.transform.Find("door(right)");
      Transform doorHandle = door.Find("doors/Handle");

      var useDoorFsm = doorHandle.GetComponent<PlayMakerFSM>();
      useDoorFsm.enabled = false;

      var doorComponent = doorHandle.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(new VehicleDoor.Config()
      {
        playerOpenTorque = -playerInteractionTorque,
        playerCloseTorque = playerInteractionTorque,
        doorCheckBreakTorque = doorCheckBreakTorque,
        hingeAxis = VehicleDoor.Axis.Z,
        door = door.gameObject,
        openHingeLimits = new JointLimits() { min = -80f, max = 0f },
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
      interiorLightFsm.SendEvent("DOOROPEN");
    }

    static void OnDoorClosed(Transform audioSource)
    {
      MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: audioSource, variationName: audioClipClose);
      interiorLightFsm.SendEvent("DOORCLOSE");
    }
  }
}
