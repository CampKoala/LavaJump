using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LavaJump
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField]
        private Slider hSlider;

       // [SerializeField]
        //private int playerHealth;



        // Start is called before the first frame update
        void Start()
        {
            hSlider = GetComponent<Slider>();
        
        }

        public void SetMaxHealth(int maxHealth) 
        { 
            hSlider.maxValue = maxHealth;

            hSlider.value = maxHealth;
        }

        public void SetHealth(int curHealth) 
        { 
               hSlider.value= curHealth;        
        }

        
    }
}
