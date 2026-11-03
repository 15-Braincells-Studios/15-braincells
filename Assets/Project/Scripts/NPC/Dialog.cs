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
        
        private void Awake()
        {
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

        private void EndDialogue()
        {
            
        }

        private IEnumerator WriteLine()
        {
            foreach (char c in DialogLines[lineIndex])
            {
                DialogueText.text += c;
                yield return new WaitForSeconds(TypeSpeed);
            }
            
            NextLinePrompt.SetActive(true);
        }

        private void NextLine()
        {
            DialogueText.text = "";
            NextLinePrompt.SetActive(false);

            lineIndex++;
            StartCoroutine(WriteLine());
        }
        
        private void OnTriggerEnter(Collider collision)
        {
            Debug.Log($"Trigger entered by {collision.name}, tag: {collision.tag}, root: {collision.transform.root.name}");
            if (collision.CompareTag("Player") && !isInteracting)
                canInteract = true;
        }

        private void OnTriggerExit(Collider collision)
        {
            if (collision.CompareTag("Player"))
                canInteract = false;
        }
    }
}