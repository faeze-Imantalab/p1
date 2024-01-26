using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    public Image fillImage;
        public int maxHealth;
        private int currentHealth;
        public GameObject gameOver;

        public void Init()
        {
            fillImage = GameObject.Find("FF").transform.GetChild(1).GetComponent<Image>();
            gameOver =  GameObject.Find("Canvas").transform.GetChild(0).gameObject;
            maxHealth = 100;


            currentHealth = maxHealth;
            SetImg();
        }

        public void SetImg()
        {
            fillImage.fillAmount = ((float) currentHealth / maxHealth);
        }

 private void OnTriggerEnter(Collider other)
    {
        print("test");
         if (other.transform.CompareTag("Enemy"))
            {
                print("1111111");
                currentHealth -= 15;
                SetImg();
                if(currentHealth <= 0)
                {
                    gameOver.SetActive(true);
                }
            }
    }
        

}
