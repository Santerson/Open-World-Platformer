using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] float SwingSpeedMultiplier = 100f;
    [SerializeField] float SwordSwingAngleMax = 165f;
    float LastLookingDir = 1;
    float Rotated = 0;

    float SwordSwingDuration;
    float SwordSwingCooldown;

    Quaternion BaseRotation;

    public bool IsSwinging { get; private set; }
    public bool IsRecoiling { get; private set; }
    

    private void Start()
    {
        SwordSwingDuration = FindObjectOfType<Player>().SwordSwingDuration;
        SwordSwingCooldown = FindObjectOfType<Player>().SwordReloadTime;

        IsSwinging = false;
        IsRecoiling = false;
        BaseRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        SwordSwingDuration = FindObjectOfType<Player>().SwordSwingDuration;
        SwordSwingCooldown = FindObjectOfType<Player>().SwordReloadTime;
        UpdateOrientSword();
        UpdateSwingSword();
    }

    private void UpdateSwingSword()
    {
        //Returns if the player isn't attacking
        if (IsSwinging == true) 
        {
            SwingSwordDown();
        }

        if (IsRecoiling == true)
        {
            SwingSwordUp();
        }
        
    }

    private void SwingSwordUp()
    {
        //Create a before rotaion variable
        float beforeRotate = transform.rotation.eulerAngles.z;

        //Roates the sword so that it will rotate however much it needs to rotate so it reaches SwordSwingAngleMax at the end of SwordSwingDuration
        transform.Rotate(0, 0, Time.deltaTime / SwordSwingCooldown * SwordSwingAngleMax * LastLookingDir);

        //Create an after rotation variable
        float afterRotate = transform.rotation.eulerAngles.z;

        //Calculate how many angles were rotated (pos or neg)
        Rotated += Mathf.Abs(beforeRotate - afterRotate);

        //Check if the angles rotated is equal to SwordSwingAngleMax
        if (Rotated > SwordSwingAngleMax)
        {
            //Reset the sword and rotation
            transform.rotation = new Quaternion(BaseRotation.x, BaseRotation.y, BaseRotation.z * LastLookingDir, BaseRotation.w);
            Rotated = 0;
            IsRecoiling = false;
        }
    }

    private void SwingSwordDown()
    {
        //Create a before rotaion variable
        float beforeRotate = transform.rotation.eulerAngles.z;

        //Roates the sword so that it will rotate however much it needs to rotate so it reaches SwordSwingAngleMax at the end of SwordSwingDuration
        transform.Rotate(0, 0, Time.deltaTime / SwordSwingDuration * SwordSwingAngleMax * -LastLookingDir);

        //Create an after rotation variable
        float afterRotate = transform.rotation.eulerAngles.z;

        //Calculate how many angles were rotated (pos or neg)
        Rotated += Mathf.Abs(beforeRotate - afterRotate);

        //Check if the angles rotated is equal to SwordSwingAngleMax
        if (Rotated > SwordSwingAngleMax)
        {
            //Reset the sword and rotation
            Rotated = 0;
            IsSwinging = false;
            IsRecoiling = true;
        }
    }

    private void UpdateOrientSword()
    {
        //Finds the player's direction
        float direction = FindObjectOfType<Player>().Direction;
        if (direction == 0)
        {   
            //Resets it if the player has put in no input
            direction = 1;
        }

        //Puts itself in the player's hand
        transform.position = new Vector2(FindObjectOfType<Player>().transform.position.x + 0.65f * direction, FindObjectOfType<Player>().transform.position.y + 0.3f);
        
        //Rotates the sword if the player has rotated
        if (direction != LastLookingDir)
        {
            //Resets last num
            LastLookingDir = direction;
            //Changes the quaternion of the rotation
            transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, -transform.rotation.z, transform.rotation.w);
        }
    }

    public void SwingSword()
    {
        IsSwinging = true;
    }

    public void HitEnemy(GameObject Enemy)
    {
        if (Enemy == null)
        {
            Debug.LogError("Somehow hit a null target");
        }
        if (Enemy.tag == "Enemy")
        {
            Destroy(Enemy);
        }
    }
}
