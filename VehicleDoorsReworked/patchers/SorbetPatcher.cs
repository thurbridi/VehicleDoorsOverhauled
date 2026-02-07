using UnityEngine;

namespace VehicleDoorsReworked
{
  static class SorbetPatcher
  {
    private static GameObject sorbet;
    private static GameObject doors;
    private static Rigidbody sorbetRigidbody;
    private static PlayMakerFSM interiorLightFsm;
    private const float playerInteractionTorque = 50f;
    private const float doorCheckBreakTorque = 75f;
    private const float angularVelocityToCloseDoor = 2.2f;
    private const string audioGroup = "CarFoley";
    private const string audioClipOpen = "sorbet_door_open";
    private const string audioClipClose = "sorbet_door_close";

    public static void Patch()
    {
      Initialize();
      // TODO: have door fsm partially active for npc compatibility
      PatchFLDoor();
      PatchFRDoor();
      PatchRLDoor();
      PatchRRDoor();

      // PatchTrunkHatch(); // TODO: make interacting with trunk hatch satisfactory
    }

    static void Initialize()
    {
      sorbet = GameObject.Find("SORBET(190-200psi)");

      sorbetRigidbody = sorbet.GetComponent<Rigidbody>();
      doors = sorbet.transform.Find("Doors").gameObject;
      interiorLightFsm = sorbet.transform.Find("LOD/InteriorLight/Use").GetComponent<PlayMakerFSM>();
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

    static void PatchFLDoor()
    {
      Transform door = doors.transform.Find("DoorFront(leftx)");
      Transform doorHandle = door.Find("FrontL/PlayerColl/Handle");

      var useDoorFsm = doorHandle.GetComponent<PlayMakerFSM>();
      useDoorFsm.enabled = false;

      PatchLeftSideDoor(doorHandle.gameObject, door.gameObject);
    }

    static void PatchFRDoor()
    {
      Transform door = doors.transform.Find("DoorFront(right)");
      Transform doorHandle = door.Find("FrontR/PlayerColl/Handle");

      var useDoorFsm = doorHandle.GetComponent<PlayMakerFSM>();
      useDoorFsm.enabled = false;

      PatchRightSideDoor(doorHandle.gameObject, door.gameObject);
    }

    static void PatchRLDoor()
    {
      Transform door = doors.transform.Find("DoorRear(leftx)");
      Transform doorHandle = door.Find("RearL/PlayerColl/Handle");

      var useDoorFsm = doorHandle.GetComponent<PlayMakerFSM>();
      useDoorFsm.enabled = false;

      PatchLeftSideDoor(doorHandle.gameObject, door.gameObject);
    }

    static void PatchRRDoor()
    {
      Transform door = doors.transform.Find("DoorRear(right)");
      Transform doorHandle = door.Find("RearR/PlayerColl/Handle");

      var useDoorFsm = doorHandle.GetComponent<PlayMakerFSM>();
      useDoorFsm.enabled = false;

      PatchRightSideDoor(doorHandle.gameObject, door.gameObject);
    }

    static void PatchTrunkHatch()
    {
      Transform hatch = sorbet.transform.Find("Hatch/Hatch");

      var useHatchFsm = hatch.GetComponent<PlayMakerFSM>();
      useHatchFsm.enabled = false;

      var doorComponent = hatch.gameObject.AddComponent<VehicleDoor>();
      doorComponent.Initialize(new VehicleDoor.Config()
      {
        playerOpenTorque = 105f,
        playerCloseTorque = -playerInteractionTorque,
        doorCheckBreakTorque = doorCheckBreakTorque,
        hingeAxis = VehicleDoor.Axis.Y,
        door = hatch.gameObject,
        openHingeLimits = new JointLimits() { min = 0f, max = 75f },
        closedHingeLimits = new JointLimits() { min = 0f, max = 0f },
        vehicleRigidbody = sorbetRigidbody,
        onDoorOpened = () =>
        {
          MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: hatch, variationName: "sorbet_bootlid_open");
        },
        onDoorClosed = () =>
        {
          MasterAudio.PlaySound3DAndForget(sType: audioGroup, sourceTrans: hatch, variationName: "sorbet_bootlid_close");
        },
        isDoorNearClosedPredicate = (doorAngle) => doorAngle < 1f,
        isPastDoorcheckAnglePredicate = (doorAngle) => doorAngle > 60f,
        isDoorFastEnoughToClosePredicate = (doorAngularVelocity) => doorAngularVelocity > angularVelocityToCloseDoor,
        angularVelocityAxis = VehicleDoor.Axis.X,
      });
    }

    static void PatchLeftSideDoor(GameObject doorHandle, GameObject door)
    {
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
        vehicleRigidbody = sorbetRigidbody,
        onDoorOpened = () => OnDoorOpened(door.transform),
        onDoorClosed = () => OnDoorClosed(door.transform),
        isDoorNearClosedPredicate = (doorAngle) => doorAngle <= 275f,
        isPastDoorcheckAnglePredicate = (doorAngle) => doorAngle > 350f,
        isDoorFastEnoughToClosePredicate = (doorAngularVelocity) => doorAngularVelocity <= -angularVelocityToCloseDoor,
        angularVelocityAxis = VehicleDoor.Axis.Y,
        doorAngleAxis = VehicleDoor.Axis.Y,
      });
    }

    static void PatchRightSideDoor(GameObject doorHandle, GameObject door)
    {
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
        vehicleRigidbody = sorbetRigidbody,
        onDoorOpened = () => OnDoorOpened(door.transform),
        onDoorClosed = () => OnDoorClosed(door.transform),
        isDoorNearClosedPredicate = (doorAngle) => doorAngle >= 265f,
        isPastDoorcheckAnglePredicate = (doorAngle) => doorAngle < 190f,
        isDoorFastEnoughToClosePredicate = (doorAngularVelocity) => doorAngularVelocity >= angularVelocityToCloseDoor,
        angularVelocityAxis = VehicleDoor.Axis.Y,
        doorAngleAxis = VehicleDoor.Axis.Y,
      });
    }
  }
}