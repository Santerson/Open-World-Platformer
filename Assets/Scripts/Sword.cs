using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] float SwingSpeedMultiplier = 100f;
    [SerializeField] float SwordSwingAngleMax = 165f;
    float LastNum = 1;
    float Rotated = 0;
    Quaternion BaseRotation;
    public bool IsSwinging { get; private set; }
    

    private void Start()
    {
        IsSwinging = false;
        BaseRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateOrientSword();
        UpdateSwingSword();
    }

    private void UpdateSwingSword()
    {
        if (IsSwinging == false) { return; }
        float beforeRotate = transform.rotation.eulerAngles.z;
        transform.Rotate(0, 0, -LastNum * SwingSpeedMultiplier * Time.deltaTime);
        float afterRotate = transform.rotation.eulerAngles.z;
        Rotated += beforeRotate - afterRotate;
        if (Mathf.Abs(Rotated) > SwordSwingAngleMax)
        {
            transform.rotation = new Quaternion(BaseRotation.x, BaseRotation.y, BaseRotation.z * LastNum, BaseRotation.w);
            Rotated = 0;
            IsSwinging = false;
        }
    }

    private void UpdateOrientSword()
    {
        float direction = FindObjectOfType<Player>().Direction;
        if (direction == 0)
        {
            direction = 1;
        }
        transform.position = new Vector2(FindObjectOfType<Player>().transform.position.x + 0.65f * direction, FindObjectOfType<Player>().transform.position.y + 0.3f);
        if (direction != LastNum)
        {
            LastNum = direction;
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
