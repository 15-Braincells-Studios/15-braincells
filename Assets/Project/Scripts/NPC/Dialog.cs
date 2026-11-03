using System;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;


namespace Project.Scripts.NPC
{
    public class Dialog : MonoBehaviour
    {
        [SerializeField] private GameObject InteractPrompt;
        
        [SerializeField] private GameObject DialogBox;
        [SerializeField] private TMP_Text DialogueText;
        [SerializeField] private GameObject NextLinePrompt;
        
        [SerializeField] private string[] DialogLines;
        [SerializeField] private float TypeSpeed = 0.0f;

        private int lineIndex = 0;
        
        private InputAction _interactAction;
        
        private bool isInteracting = false;
        private bool canInteract = false;
        public bool IsInteracting => isInteracting;
        
        private void Awake()
        {
            // using the new input systems interact button E for keyboard
            _interactAction = InputSystem.actions.FindAction("Interact");
        }

        private void Update()
        {
            InteractPrompt.SetActive(canInteract);

            if (canInteract && !isInteracting && _interactAction.WasPressedThisFrame())
                StartDialogue();

            if (isInteracting && NextLinePrompt.activeInHierarchy && _interactAction.WasPressedThisFrame())
                NextLine();
        }

        // start dialogue and turn on dialogue panels while turning off interaction panel
        private void StartDialogue()
        {
            canInteract = false;
            isInteracting = true;

            DialogueText.text = "";
            NextLinePrompt.SetActive(false);
            DialogBox.SetActive(true);
            
            lineIndex = 0;
            StartCoroutine(WriteLine());
        }

        // stop all dialogue and turn off all panels
        private void EndDialogue()
        {
            StopAllCoroutines();
            isInteracting = false;
            DialogueText.text = "";
            NextLinePrompt.SetActive(false);
            DialogBox.SetActive(false);
            canInteract = true; 
        }

        // write line in typewriter way for each character in the sentence
        private IEnumerator WriteLine()
        {
            foreach (char c in DialogLines[lineIndex])
            {
                DialogueText.text += c;
                yield return new WaitForSeconds(TypeSpeed);
            }
            
            NextLinePrompt.SetActive(true);
        }

        // counts how many lines there are and ends dialogue when the end is reached
        private void NextLine()
        {
            DialogueText.text = "";
            NextLinePrompt.SetActive(false);

            lineIndex++;
            
            if (lineIndex >= DialogLines.Length)
            {
                EndDialogue();
                return;
            }
            StartCoroutine(WriteLine());
        }
        
        private void OnTriggerEnter(Collider collision)
        {
            // if npc is interacting with a player tag activate dialogue lines
            if (collision.CompareTag("Player") && !isInteracting)
                canInteract = true;
        }

        private void OnTriggerExit(Collider collision)
        {
            if (!collision.CompareTag("Player")) return;

            canInteract = false;
            
            if (isInteracting)
                EndDialogue();
            canInteract = false; 
        }
    }
}