using GenClue = FifteenBraincells.MurderGenerator.Output.Clue;
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
        
        [Header("Generator character id, like chef or king")]
        [SerializeField] private string characterId = "";
        
        // For every line: the generator clue it came from, or null for a fixed line from the JSON.
        private readonly List<GenClue> clues = new();
        // Generated clues this NPC has already put in the journal, so talking twice doesn't add them twice.
        private readonly HashSet<GenClue> addedToJournal = new();
        
        [SerializeField] private ClueManager clueManager;
        [SerializeField] private GameObject InteractPrompt;
        
        [SerializeField] private GameObject DialogBox;
        [SerializeField] private TMP_Text DialogueText;
        [SerializeField] private GameObject NextLinePrompt;
        
        [SerializeField] private float TypeSpeed = 0.0f;

        private int lineIndex = 0;
        
        private InputAction _interactAction;
        private ConversationListUI conversationListUI;
        private bool conversationSaved = false;
        public PlayerMovement player;
        public Camera PlayerCamera;
        public LayerMask playerLayer;
        int originalMask;

        
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
            originalMask = PlayerCamera.cullingMask;
            PlayerCamera.cullingMask &= ~playerLayer;
            _lines.Clear();
            _lineKeys.Clear();
            _groups.Clear();
            clues.Clear();

            for (int i = 1; Localization.Has($"{dialogId}_line_{i}"); i++)
            {
                string key = $"{dialogId}_line_{i}";
                _groups.Add(Localization.GetGroup(key));
                _lineKeys.Add(key);
                _lines.Add(Localization.Get(key));
                clues.Add(null);
            }
            
            // Generated lines from the current case
            var caseManager = CaseManager.Instance;
            Debug.Log("Conversation Character ID: " + characterId);
            Debug.Log("Loaded dialogue groups: " + _groups.Count);
            if (_groups.Count > 0)
            {
                string speakerName = !string.IsNullOrEmpty(characterId)
                    ? caseManager.GetDisplayName(characterId)
                    : _groups[0];
                Debug.Log("Speaker name: " + speakerName);
                if (conversationListUI == null)
                    conversationListUI = FindFirstObjectByType<ConversationListUI>();
            }
            if (conversationListUI == null)
                conversationListUI = FindFirstObjectByType<ConversationListUI>();

            if (!string.IsNullOrEmpty(characterId) && caseManager != null)
            {
                string speakerName = caseManager.GetDisplayName(characterId);
                conversationListUI.AddConversation(characterId, speakerName);
            }
            if (caseManager != null && !string.IsNullOrEmpty(characterId))
            {
                string speakerName = caseManager.GetDisplayName(characterId);
                var generatedLines = caseManager.GetLinesFor(characterId);
                Debug.Log("Generated conversation lines: " + generatedLines.Count);
                foreach (GenClue clue in generatedLines)
                {
                    _groups.Add(speakerName);           // the journal shows who said it
                    _lineKeys.Add("");                  // no localization key
                    _lines.Add(caseManager.Render(clue));
                    clues.Add(clue);
                }
            }
            if (conversationListUI != null)
            {
                foreach (string line in _lines)
                {
                    conversationListUI.AddConversationLine(characterId, line);
                }
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
            PlayerCamera.cullingMask = originalMask; 
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

            // Is this line a clue?
            GenClue generated = clues[lineIndex];                                                   // null = fixed line from the JSON
            bool isGeneratedClue = generated != null && addedToJournal.Add(generated);              // generated, and not in the journal yet
            bool isFixedClue = generated == null && Localization.IsClue(_lineKeys[lineIndex]);      // fixed line marked isClue

            if (isGeneratedClue || isFixedClue)
            {
                // Fixed lines use their localization key. Generated lines have no key, so they get their own id
                // (like "chef_Alibi_sharpening_the_knives"), otherwise ClueManager sees them all as the same clue.
                string clueID = generated != null
                    ? $"{characterId}_{generated.Category}_{generated.SourceId}"
                    : _lineKeys[lineIndex];

                bool alreadyAdded = !string.IsNullOrEmpty(clueID) &&
                                    clueManager.clues.Exists(clue => clue.clueID == clueID);

                if (!alreadyAdded)
                {
                    ClueData clue = new ClueData
                    {
                        clueID = clueID,
                        clueName = _groups[lineIndex],
                        description = _lines[lineIndex],
                        clueImage = null,
                        clueType = "Dialogue",
                        generatorClue = generated
                    };

                    clueManager.AddClue(clue);
                }
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