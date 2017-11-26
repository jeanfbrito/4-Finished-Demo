using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace RPG.Characters
{
    public class HealthSystem : MonoBehaviour
    {
        [SerializeField] float maxHealthPoints = 100f;
        [SerializeField] Image healthBar;
        [SerializeField] Text nameBar;
        [SerializeField] AudioClip[] damageSounds;
        [SerializeField] AudioClip[] deathSounds;
        [SerializeField] float deathVanishSeconds = 2.0f;

        [SerializeField] HUDText hudText;

        const string DEATH_TRIGGER = "Death";

        float currentHealthPoints; 
        Animator animator;
        AudioSource audioSource;
        Character characterMovement;

        public float healthAsPercentage { get { return currentHealthPoints / maxHealthPoints; } }

        void Start()
        {
            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            characterMovement = GetComponent<Character>();
            hudText = GetComponent<HUDText>();

            currentHealthPoints = maxHealthPoints;
        }

        void Update()
        {
            UpdateHealthBar();
        }
       
        void UpdateHealthBar()
        {
            if (healthBar) // Enemies may not have health bars to update
            {
                healthBar.fillAmount = healthAsPercentage;
                if (healthAsPercentage > 0.95)
                {
                    healthBar.color = new Color32(0, 200, 0, 255);
                    nameBar.color = new Color32(0, 200, 0, 255);
                }
                else if (healthAsPercentage > 0.65)
                {
                    healthBar.color = new Color32(76, 153, 76, 255);
                    nameBar.color = new Color32(76, 153, 76, 255);
                }
                else if (healthAsPercentage > 0.30)
                {
                    healthBar.color = new Color32(164, 164, 0, 255);
                    nameBar.color = new Color32(164, 164, 0, 255);
                }
                else if(healthAsPercentage > 0.1)
                {
                    healthBar.color = new Color32(224, 51, 51, 255);
                    nameBar.color = new Color32(224, 51, 51, 255);
                }
                else 
                {
                    healthBar.color = new Color32(128, 0, 0, 255);
                    nameBar.color = new Color32(128, 0, 0, 255);
                }
            }
        }

        public void TakeDamage(float damage)
        {
            hudText.Add(0 - damage, Color.red, 0f);
            bool characterDies = (currentHealthPoints - damage <= 0);
            currentHealthPoints = Mathf.Clamp(currentHealthPoints - damage, 0f, maxHealthPoints);
            var clip = damageSounds[UnityEngine.Random.Range(0, damageSounds.Length)];
            audioSource.PlayOneShot(clip);
            if (characterDies)
            {
                StartCoroutine(KillCharacter());
            }
        }

        public void Heal(float points)
        {
            currentHealthPoints = Mathf.Clamp(currentHealthPoints + points, 0f, maxHealthPoints);
        }

        IEnumerator KillCharacter()
        {
            characterMovement.Kill();
            animator.SetTrigger(DEATH_TRIGGER);

            audioSource.clip = deathSounds[UnityEngine.Random.Range(0, deathSounds.Length)];
            audioSource.Play(); // overrind any existing sounds
            yield return new WaitForSecondsRealtime(audioSource.clip.length);

            var playerComponent = GetComponent<PlayerControl>();
            if (playerComponent && playerComponent.isActiveAndEnabled) // relying on lazy evaluation
            {
                SceneManager.LoadScene(0);
            }
            else // assume is enemy fr now, reconsider on other NPCs
            {
                DestroyObject(gameObject, deathVanishSeconds);
            }
        }
    }
}