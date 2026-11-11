using System;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Project.Scripts.Core;
using Unity.Cinemachine;


namespace Project.Scripts.NPC
{
    public class Dialog : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private CinemachineCamera dialogueCamera; // CinemachineVirtualCamera in older versions
        [SerializeField] private int activePriority = 20;
        [SerializeField] private int inactivePriority = 0;
        
        [Header("Connect DialogID to the correct json name")]
        [SerializeField] private string dialogId = "guard_1";
        private readonly List<string> _lines = new();
        private readonly List<string> _lineKeys = new();
        private readonly List<string> _groups = new();
        [SerializeField] private ClueManager clueManager;
        [SerializeField] private GameObject InteractPrompt;
        
        [SerializeField] private GameObject DialogBox;
        [SerializeField] private TMP_Text DialogueText;
        [SerializeField] private GameObject NextLinePrompt;
        
        [SerializeField] private float TypeSpeed = 0.0f;

        private int lineIndex = 0;
        
        private InputAction _interactAction;
        public PlayerMovement player;
        
        private bool isInteracting = false;
        private bool canInteract = false;
        public bool IsInteracting => isInteracting;
        
        private void Awake()
        {
            // using the new input systems interact button E for keyboard
            _interactAction = InputSystem.actions.FindAction("Interact");
            dialogueCamera.Priority = inactivePriority;
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
            player.SetCanMove(false);
            _lines.Clear();
            _lineKeys.Clear();
            _groups.Clear();

            for (int i = 1; Localization.Has($"{dialogId}_line_{i}"); i++)
            {
                string key = $"{dialogId}_line_{i}";
                _groups.Add(Localization.GetGroup(key));
                _lineKeys.Add(key);
                _lines.Add(Localization.Get(key));
            }

            if (_lines.Count == 0) return;  
            
            canInteract = false;
            isInteracting = true;
            dialogueCamera.Priority = activePriority;
            DialogueText.text = "";
            NextLinePrompt.SetActive(false);
            DialogBox.SetActive(true);
            lineIndex = 0;
            StartCoroutine(WriteLine());
        }

        // stop all dialogue and turn off all panels
        private void EndDialogue()
        {
            player.SetCanMove(true);
            StopAllCoroutines();
            isInteracting = false;
            dialogueCamera.Priority = inactivePriority;
            DialogueText.text = "";
            NextLinePrompt.SetActive(false);
            DialogBox.SetActive(false);
            canInteract = true; 
        }

        // write line in typewriter way for each character in the sentence
        private IEnumerator WriteLine()
        {
            foreach (char c in _lines[lineIndex])
            {
                DialogueText.text += c;
                yield return new WaitForSeconds(TypeSpeed);
            }
            
            NextLinePrompt.SetActive(true);
            // Add the dialogue line to the clues list if it is marked as important
            if (Localization.IsClue(_lineKeys[lineIndex]))
            {
                ClueData clue = new ClueData
                {
                    clueName = _groups[lineIndex],
                    description = _lines[lineIndex],
                    clueImage = null,
                    clueType = "Dialogue"
                };

                clueManager.AddClue(clue);
            }
        }

        // counts how many lines there are and ends dialogue when the end is reached
        private void NextLine()
        {
            DialogueText.text = "";
            NextLinePrompt.SetActive(false);

            lineIndex++;
            
            if (lineIndex >= _lines.Count)
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