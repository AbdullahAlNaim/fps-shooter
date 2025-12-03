using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    public static WeaponManager Instance { get; set; }

    public List<GameObject> weaponSlots;

    public GameObject activeWeaponSlot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (!IsOwner) { enabled = false; return; }

        activeWeaponSlot = weaponSlots[0];
    }

    private void Update()
    {
        if (!IsOwner) return;

        foreach (GameObject weaponSlot in weaponSlots)
        {
            if (weaponSlot == activeWeaponSlot)
            {
                weaponSlot.SetActive(true);
            }
            else
            {
                weaponSlot.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchActiveSlot(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchActiveSlot(1);
        }
    }

    public void PickUpWeapon(GameObject pickedUpWeapon)
    {
        AddWeaponIntoActiveSlot(pickedUpWeapon);
    }

    private void AddWeaponIntoActiveSlot(GameObject pickedUpWeapon)
    {
        DropCurrentWeapon(pickedUpWeapon);

        //pickedUpWeapon.transform.SetParent(activeWeaponSlot.transform, false);

        //Weapon weapon = pickedUpWeapon.GetComponent<Weapon>();

        //pickedUpWeapon.transform.localPosition = new Vector3(weapon.spawnPosition.x, weapon.spawnPosition.y, weapon.spawnPosition.z);
        //pickedUpWeapon.transform.localRotation = Quaternion.Euler(weapon.spawnRotation.x, weapon.spawnRotation.y, weapon.spawnRotation.z);

        //weapon.isActiveWeapon = true;
        var weapon = pickedUpWeapon.GetComponent<Weapon>();

        // Ensure follower exists
        var follower = pickedUpWeapon.GetComponent<WeaponFollower>();
        if (follower == null) follower = pickedUpWeapon.AddComponent<WeaponFollower>();

        // The placeholder the weapon should follow (under your player)
        follower.target = activeWeaponSlot.transform;

        // Use your existing spawn offsets (interpreted as local offsets)
        follower.positionOffset = weapon.spawnPosition;               // Vector3
        follower.eulerOffset = weapon.spawnRotation;               // Vector3 (Euler)

        // Set an initial snap so it looks right immediately this frame
        pickedUpWeapon.transform.position = follower.target.position + follower.target.TransformVector(follower.positionOffset);
        pickedUpWeapon.transform.rotation = follower.target.rotation * Quaternion.Euler(follower.eulerOffset);

        weapon.isActiveWeapon = true;
    }

    private void DropCurrentWeapon(GameObject pickedUpWeapon)
    {
        if (activeWeaponSlot.transform.childCount > 0)
        {
            var weaponToDrop = activeWeaponSlot.transform.GetChild(0).gameObject;
            //--
            var w = weaponToDrop.GetComponent<Weapon>();
            if (w) w.isActiveWeapon = false;

            var follower = weaponToDrop.GetComponent<WeaponFollower>();
            if (follower) follower.target = null;
            //==
            //weaponToDrop.GetComponent<Weapon>().isActiveWeapon = false;

            //weaponToDrop.transform.SetParent(pickedUpWeapon.transform.parent);
            //weaponToDrop.transform.localPosition = pickedUpWeapon.transform.localPosition;
            //weaponToDrop.transform.localRotation = pickedUpWeapon.transform.localRotation;
        }
    }

    public void SwitchActiveSlot(int slotNumber)
    {
        if (activeWeaponSlot.transform.childCount > 0)
        {
            Weapon currentWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            currentWeapon.isActiveWeapon = false;
        }

        activeWeaponSlot = weaponSlots[slotNumber];

        //------------------
        // If your active weapon object isn’t literally a child anymore,
        // you can track it another way. For quick compatibility:
        foreach (var w in FindObjectsOfType<Weapon>())
        {
            if (!w.isActiveWeapon) continue;
            var follower = w.GetComponent<WeaponFollower>();
            if (follower != null) follower.target = activeWeaponSlot.transform;
        }
        //==============================================


        if (activeWeaponSlot.transform.childCount > 0)
        {
            Weapon newWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            newWeapon.isActiveWeapon = true;
        }
    }

}


//using System.Collections.Generic;
//using Unity.Netcode;
//using UnityEngine;

//public class WeaponManager : NetworkBehaviour
//{
//    [Header("Holder slots are child Transforms under the player (no NetworkObject needed)")]
//    [SerializeField] private List<Transform> weaponSlots;
//    [SerializeField] private int activeSlotIndex = 0;

//    private Transform ActiveSlot => (weaponSlots != null && weaponSlots.Count > 0)
//        ? weaponSlots[Mathf.Clamp(activeSlotIndex, 0, weaponSlots.Count - 1)]
//        : null;

//    public IReadOnlyList<Transform> WeaponSlots => weaponSlots;
//    public Transform ActiveSlotTransform => ActiveSlot;

//    private void Start()
//    {
//        // Only the local owner drives input/UI for their own player
//        if (!IsOwner) { enabled = false; return; }
//    }

//    private void Update()
//    {
//        if (!IsOwner) return;

//        // Example slot switching
//        if (Input.GetKeyDown(KeyCode.Alpha1)) activeSlotIndex = 0;
//        if (Input.GetKeyDown(KeyCode.Alpha2)) activeSlotIndex = 1;
//    }

//    /// <summary>
//    /// Called by your interaction code when the local player presses F on a weapon.
//    /// </summary>
//    public void RequestPickup(NetworkObject weaponNO)
//    {
//        if (!IsOwner || weaponNO == null) return;

//        var slot = ActiveSlot;
//        if (!slot)
//        {
//            Debug.LogWarning("WeaponManager: ActiveSlot is not set.");
//            return;
//        }

//        // Send the slot world pose to the server so it can snap the weapon after parenting
//        RequestPickupServerRpc(
//            weaponNO,
//            slot.position,
//            slot.rotation
//        );
//    }

//    /// <summary>
//    /// Server-side: parent the weapon NetworkObject under the player's NetworkObject,
//    /// then snap it to the provided world pose (the holder slot pose).
//    /// </summary>
//    //[ServerRpc(RequireOwnership = false)] // deprecated used whats below
//    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
//    private void RequestPickupServerRpc(
//        NetworkObjectReference weaponRef,
//        Vector3 slotWorldPos,
//        Quaternion slotWorldRot,
//        ServerRpcParams rpcParams = default)
//    {
//        if (!weaponRef.TryGet(out var weaponNO) || weaponNO == null) return;

//        // (Optional) Validate: distance/line-of-sight using rpcParams.Receive.SenderClientId

//        // Parent the weapon to THIS player's NetworkObject (this WeaponManager is on the player)
//        var playerNO = NetworkObject; // the player's NetworkObject
//        if (!playerNO)
//        {
//            Debug.LogWarning("WeaponManager: Player NetworkObject missing.");
//            return;
//        }

//        // If the weapon will be owner-driven (e.g., ClientNetworkTransform), give them ownership
//        // weaponNO.ChangeOwnership(rpcParams.Receive.SenderClientId);

//        // Re-parent on the server (immediate parent must be a NetworkObject)
//        bool parented = weaponNO.TrySetParent(playerNO, worldPositionStays: false);
//        if (!parented)
//        {
//            Debug.LogWarning("WeaponManager: TrySetParent failed (is parent a valid NetworkObject?).");
//            return;
//        }

//        // Snap to the slot's pose (world space)
//        var t = weaponNO.transform;
//        t.SetPositionAndRotation(slotWorldPos, slotWorldRot);

//        // Optional: disable physics while held
//        var rb = weaponNO.GetComponent<Rigidbody>();
//        if (rb) { rb.isKinematic = true; rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

//        // Optional: mark active on server or via NetworkVariable
//        var weapon = weaponNO.GetComponent<Weapon>();
//        if (weapon) weapon.isActiveWeapon = true;

//        // (Optional) notify the owning client to run any local-only visual setup (e.g., enabling hands UI)
//        // PickupFeedbackClientRpc(new ClientRpcParams {
//        //     Send = new ClientRpcSendParams { TargetClientIds = new[] { rpcParams.Receive.SenderClientId } }
//        // });
//    }

//    /// <summary>
//    /// Example: drop the currently held weapon from the active slot (server-side).
//    /// Call this via a ServerRpc from the owner when they press 'G', etc.
//    /// </summary>
//    //[ServerRpc(RequireOwnership = false)]
//    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
//    public void DropActiveWeaponServerRpc(Vector3 dropWorldPos, Quaternion dropWorldRot, ServerRpcParams rpcParams = default)
//    {
//        var playerNO = NetworkObject;
//        if (!playerNO) return;

//        // Find a child weapon under this player (simple example: first NetworkObject child that is a weapon)
//        NetworkObject weaponNO = null;
//        foreach (Transform child in playerNO.transform)
//        {
//            // Heuristic: has NetworkObject and Weapon component
//            var candidateNO = child.GetComponent<NetworkObject>();
//            if (candidateNO && candidateNO.GetComponent<Weapon>())
//            {
//                weaponNO = candidateNO;
//                break;
//            }
//        }

//        if (!weaponNO) return;

//        // Unparent (null) so it returns to scene root (or a neutral network parent)
//        weaponNO.TrySetParent((NetworkObject)null, worldPositionStays: true);

//        // Place at drop pose
//        weaponNO.transform.SetPositionAndRotation(dropWorldPos, dropWorldRot);

//        // Re-enable physics
//        var rb = weaponNO.GetComponent<Rigidbody>();
//        if (rb) rb.isKinematic = false;

//        // (Optional) return ownership to server
//        // weaponNO.RemoveOwnership();
//    }
//}
////////////////////////////////////////////////////////////////////////////////////////////

