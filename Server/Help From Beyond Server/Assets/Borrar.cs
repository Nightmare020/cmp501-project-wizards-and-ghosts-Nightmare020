using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Borrar : MonoBehaviour
{
    static Borrar instance;
    void Awake()
    {
        if(instance != null)
        {
            Destroy(this.gameObject);
            
        }
        else{
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);

    }

}
