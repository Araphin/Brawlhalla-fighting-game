using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons : MonoBehaviour
{
   
    public string randomWeapon()
    {
        List<string> list = new List<string>(){ "spear", "sword", "hammer", "axe" };
        int rnd = UnityEngine.Random.Range (0, list.Count);
        Destroy(gameObject, 0.1f);
        return list[rnd];
        
    }


}
