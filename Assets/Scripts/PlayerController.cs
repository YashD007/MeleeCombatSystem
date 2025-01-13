using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator playerAnim;

    // Weapons
    [SerializeField] private Weapon[] weapons; // Array of weapons
    private int currentWeaponIndex = 0;
    private Weapon currentWeapon;

    public bool isEquipping;
    public bool isEquipped;

    public bool isBlocking;
    public bool isKicking;
    public bool isAttacking;
    private float timeSinceAttack;
    public int currentAttack = 0;

    [Header("Throwable Weapon Settings")]
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwForce = 20f;

    private void Start()
    {
        EquipWeapon(0); // Equip the first weapon by default
    }

    private void Update()
    {
        timeSinceAttack += Time.deltaTime;

        if (currentWeapon.isThrowable)
        {
            ThrowWeapon(); // Check for throwable weapon input
        }
        else
        {
            Attack(); // Handle melee attacks
        }

        Equip();
        Block();
        Kick();

        // Switch weapon using mouse scroll wheel
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            SwitchWeapon(1);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            SwitchWeapon(-1);
        }
    }

    private void Equip()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerAnim.GetBool("Grounded"))
        {
            isEquipping = true;
            playerAnim.SetTrigger("Equip");
        }
    }

    public void ActiveWeapon()
{
    if (!isEquipped)
    {
        currentWeapon.weaponInHand.SetActive(true);
        currentWeapon.weaponOnShoulder.SetActive(false);
        isEquipped = true;
    }
    else
    {
        currentWeapon.weaponInHand.SetActive(false);
        currentWeapon.weaponOnShoulder.SetActive(true);
        isEquipped = false;
    }
}


    public void Equipped()
    {
        isEquipping = false;
    }

    private void Block()
    {
        if (Input.GetKey(KeyCode.Mouse1) && playerAnim.GetBool("Grounded"))
        {
            playerAnim.SetBool("Block", true);
            isBlocking = true;
        }
        else
        {
            playerAnim.SetBool("Block", false);
            isBlocking = false;
        }
    }

    public void Kick()
    {
        if (Input.GetKey(KeyCode.LeftControl) && playerAnim.GetBool("Grounded"))
        {
            playerAnim.SetBool("Kick", true);
            isKicking = true;
        }
        else
        {
            playerAnim.SetBool("Kick", false);
            isKicking = false;
        }
    }

    private void Attack()
{
    // Ensure the player has a weapon equipped before attacking
    if (isEquipped && Input.GetMouseButtonDown(0) && playerAnim.GetBool("Grounded") && timeSinceAttack > 0.8f)
    {
        currentAttack++;
        isAttacking = true;

        if (currentAttack > 3)
            currentAttack = 1;

        if (timeSinceAttack > 1.0f)
            currentAttack = 1;

        playerAnim.SetTrigger("Attack" + currentAttack);
        timeSinceAttack = 0;
    }
}


    private void ThrowWeapon()
{
    if (Input.GetMouseButtonDown(0) && playerAnim.GetBool("Grounded"))
    {
        // Check if the weaponPrefab is assigned before instantiating
        if (currentWeapon.weaponPrefab == null)
        {
            Debug.LogError("No weapon prefab assigned for " + currentWeapon.weaponName);
            return; // Exit if no prefab is assigned
        }

        // Instantiate the throwable weapon at the throwPoint's position and rotation
        GameObject thrownWeapon = Instantiate(currentWeapon.weaponPrefab, throwPoint.position, Quaternion.identity);

        // Get the Rigidbody of the thrown weapon
        Rigidbody weaponRb = thrownWeapon.GetComponent<Rigidbody>();
        if (weaponRb != null)
        {
            // Apply force based on throwPoint's forward direction or player's forward direction
            Vector3 throwDirection = transform.forward; // Or use transform.forward if you want the player to throw along their body axis
            weaponRb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }

        // Trigger the same attack animation as for melee attacks
        playerAnim.SetTrigger("Attack" + currentAttack);

        // Optionally reset the attack counter for consistency (or add a throw-specific condition)
        currentAttack++;
        if (currentAttack > 3)
            currentAttack = 1;

        // Prevent melee attack logic from being triggered while throwing
        isAttacking = false;

        // Destroy the thrown weapon after 5 seconds
        Destroy(thrownWeapon, 2f);
    }
}




    public void ResetAttack()
    {
        isAttacking = false;
    }

    private void EquipWeapon(int index)
    {
        if (currentWeapon != null)
        {
            currentWeapon.weaponInHand.SetActive(false);
            currentWeapon.weaponOnShoulder.SetActive(true);
        }

        currentWeaponIndex = index;
        currentWeapon = weapons[currentWeaponIndex];

        if (isEquipped)
        {
            currentWeapon.weaponInHand.SetActive(true);
            currentWeapon.weaponOnShoulder.SetActive(false);
        }
        else
        {
            currentWeapon.weaponInHand.SetActive(false);
            currentWeapon.weaponOnShoulder.SetActive(true);
        }
    }

    private void SwitchWeapon(int direction)
    {
        int newIndex = (currentWeaponIndex + direction + weapons.Length) % weapons.Length;
        EquipWeapon(newIndex);
    }
}

[System.Serializable]
public class Weapon
{
    public string weaponName;
    public GameObject weaponInHand;
    public GameObject weaponOnShoulder;
    public GameObject weaponPrefab; // Add the prefab for the throwable weapon
    public bool isThrowable; // Indicates if the weapon is throwable (like a knife, grenade, etc.)
}
