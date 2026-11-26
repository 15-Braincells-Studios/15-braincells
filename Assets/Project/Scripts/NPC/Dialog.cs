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
        public string CharacterId => characterId;
        
        [Header("Interaction")]
        [SerializeField] private float interactDistance = 4f;
        [SerializeField] private float lookRadius = 0.8f;
        
        // For every line: the generator clue it came from, or null for a fixed line from the JSON.
        private readonly List<GenClue> clues = new();
        // Generated clues this NPC has already put in the journal, so talking twice doesn't add them twice.
        private readonly HashSet<GenClue> addedToJournal = new();
        
        [SerializeField] private float npcHeight = 1.8f;
        
        [SerializeField] private ClueManager clueManager;
        [SerializeField] private GameObject InteractPrompt;
        
        [SerializeField] private GameObject DialogBox;
        [SerializeField] private TMP_Text DialogueText;
        [SerializeField] private GameObject NextLinePrompt;
        
        [SerializeField] private float TypeSpeed = 0.02f;

        private int lineIndex = 0;
        
        private InputAction _interactAction;
        private ConversationListUI conversationListUI;
        private bool conversationSaved = false;
        private bool isTyping;
        public PlayerMovement player;
        public Camera PlayerCamera;
        public LayerMask playerLayer;
        int originalMask;

        
        private bool isInteracting = false;
        private bool canInteract = false;
        public bool IsInteracting => isInteracting;

        // All NPCs share one "press E to talk" prompt, so it shows while at least one NPC can be talked to.
        private static readonly HashSet<Dialog> NpcsInRange = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => NpcsInRange.Clear();

        private void Awake()
        {
            // using the new input systems interact button E for keyboard
            _interactAction = InputSystem.actions.FindAction("Interact");
            dialogueCamera.Priority = inactivePriority;
        }

        private void OnDisable()
        {
            SetCanInteract(false);
        }

        // Only change the shared prompt when this NPC's state changes, instead of every frame
        private void SetCanInteract(bool value)
        {
            canInteract = value;
            if (value) NpcsInRange.Add(this);
            else NpcsInRange.Remove(this);

            if (InteractPrompt != null)
                InteractPrompt.SetActive(NpcsInRange.Count > 0);

            // Hide the crosshair while the "press E to talk" prompt is showing
            if (value) PlayerHud.HideCrosshair(this);
            else PlayerHud.ShowCrosshair(this);
        }

        private void Update()
        {
            if (Time.timeScale == 0f) return;

            if (!isInteracting)
            {
                bool shouldInteract = IsLookedAt();
                if (shouldInteract != canInteract)
                    SetCanInteract(shouldInteract);
            }

            bool pressed = _interactAction.WasPressedThisFrame();

            if (canInteract && !isInteracting && pressed)
            {
                StartDialogue();
            }
            else if (isInteracting && pressed)
            {
                if (isTyping) SkipTyping();
                else NextLine();
            }
        }
        
        private void SkipTyping()
        {
            StopAllCoroutines();
            DialogueText.text = _lines[lineIndex];
            FinishLine();
        }
        
        private void FinishLine()
        {
            isTyping = false;
            NextLinePrompt.SetActive(true);
        }

        private bool IsLookedAt()
        {
            Transform cam = PlayerCamera.transform;
            Vector3 bottom = transform.position;
            Vector3 top = bottom + Vector3.up * npcHeight;

            for (int i = 0; i <= 4; i++)
            {
                Vector3 point = Vector3.Lerp(bottom, top, i / 4f);

                if (Vector3.Distance(player.transform.position, point) > interactDistance) continue;

                Vector3 toPoint = point - cam.position;
                if (Vector3.Dot(cam.forward, toPoint) <= 0f) continue;
                if (Vector3.Cross(cam.forward, toPoint).magnitude <= lookRadius)
                    return true;
            }
            return false;
        }

        // start dialogue and turn on dialogue panels while turning off interaction panel
        private void StartDialogue()
        {
            player.SetCanMove(false);
            SuspectData suspect = System.Array.Find(
                Localization.Suspects,
                s => s.character_id == characterId
            );

            bool wasAlreadyMet = suspect != null && suspect.has_been_met;

            Localization.MarkSuspectAsMet(characterId);

            if (!wasAlreadyMet)
            {
                JournalNotificationManager.Instance.ShowNotification(
                    "Suspect in journal is updated"
                );
            }

            SuspectListUI suspectList = FindFirstObjectByType<SuspectListUI>();
            if (suspectList != null)
            {
                suspectList.RefreshSuspectEntries();
            }
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
            if (_groups.Count > 0)
            {
                string speakerName = !string.IsNullOrEmpty(characterId)
                    ? caseManager.GetDisplayName(characterId)
                    : _groups[0];
                Debug.Log("Speaker name: " + speakerName);
            }

            // Also find the conversation list while the journal is closed (inactive)
            if (conversationListUI == null)
                conversationListUI = FindFirstObjectByType<ConversationListUI>(FindObjectsInactive.Include);

            // No journal in the scene: skip the conversation list, but keep going so the clues still get added
            if (conversationListUI != null && !string.IsNullOrEmpty(characterId) && caseManager != null)
            {
                string speakerName = caseManager.GetDisplayName(characterId);
                conversationListUI.AddConversation(characterId, speakerName);
            }
            if (caseManager != null && !string.IsNullOrEmpty(characterId))
            {
                string speakerName = caseManager.GetDisplayName(characterId);
                var generatedLines = caseManager.GetLinesFor(characterId);
                foreach (GenClue clue in generatedLines)
                {
                    _groups.Add(speakerName);
                    _lineKeys.Add("");
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
            
            SetCanInteract(false);
            isInteracting = true;
            PlayerHud.Hide(this);
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
            PlayerHud.Show(this);
            dialogueCamera.Priority = inactivePriority;
            DialogueText.text = "";
            NextLinePrompt.SetActive(false);
            DialogBox.SetActive(false);
        }

        // write line in typewriter way for each character in the sentence
        private IEnumerator WriteLine()
        {
            isTyping = true;
            DialogueText.text = "";

            foreach (char c in _lines[lineIndex])
            {
                DialogueText.text += c;
                yield return new WaitForSeconds(TypeSpeed);
            }

            FinishLine();
            NextLinePrompt.SetActive(true);

            // Is this line a clue?
            // GenClue generated = clues[lineIndex];                                                   // null = fixed line from the JSON
            // bool isGeneratedClue = generated != null && addedToJournal.Add(generated);              // generated, and not in the journal yet
            // bool isFixedClue = generated == null && Localization.IsClue(_lineKeys[lineIndex]);      // fixed line marked isClue
            //
            // if (isGeneratedClue || isFixedClue)
            // {
            //     // Fixed lines use their localization key. Generated lines have no key, so they get their own id
            //     // (like "chef_Alibi_sharpening_the_knives"), otherwise ClueManager sees them all as the same clue.
            //     string clueID = generated != null
            //         ? $"{characterId}_{generated.Category}_{generated.SourceId}"
            //         : _lineKeys[lineIndex];
            //
            //     bool alreadyAdded = !string.IsNullOrEmpty(clueID) &&
            //                         clueManager.clues.Exists(clue => clue.clueID == clueID);
            //
            //     if (!alreadyAdded)
            //     {
            //         ClueData clue = new ClueData
            //         {
            //             clueID = clueID,
            //             clueName = _groups[lineIndex],
            //             description = _lines[lineIndex],
            //             clueImage = null,
            //             clueType = "Dialogue",
            //             generatorClue = generated
            //         };
            //
            //         clueManager.AddClue(clue);
            //     }
            // }
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
    }
}