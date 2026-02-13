using UnityEngine;

namespace VehicleDoorsOverhauled
{
  static class HeppaPatcher
  {
    static Transform door;
    static Transform doorHandle;
    static Rigidbody vehicleRigidbody;
    private const float playerInteractionTorque = 50f;
    private const float doorCheckBreakTorque = 75f;
    private const float angularVelocityToCloseDoor = 2.2f;
    const string audioGroup = "CarFoley";
    const string audioClipOpen = "open_door1";
    const string audioClipClose = "close_door1";

    public static void Patch()
    {
      Initialize();
      PatchRightDoor();
    }

    static void Initialize()
    {
      Transform vehicle = GameObject.Find("TRAFFIC").transform.Find("VehiclesDirtRoad/Rally/HEPPA");
      door = vehicle.Find("DriverDoors/doorr");
      doorHandle = vehicle.Find("DriverDoors/doorr/Pivot/Handle");
      vehicleRigidbody = vehicle.GetComponent<Rigidbody>();
    }

    static void PatchRightDoor()
    {
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
