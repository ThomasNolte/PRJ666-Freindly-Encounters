﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardPanel : MonoBehaviour
{
    public const int MAXCARDS = 4;

    public NetworkCard card;

    public GameObject interactionCardPanel;
    public GameObject movementCardPanel;
    public GameObject deck;
    public Transform parentCardPanels;

    public Text typeCardText;

    private int cardSelectedPosition = -1;
    private bool finishInteraction = false;
    private bool interacting = false;
    private bool drawnCard = false;

    private GameObject[] interactionPanels;
    private GameObject[] movementPanels;
    private List<GameObject[]> movementHand = new List<GameObject[]>();
    private List<GameObject[]> interactionHand = new List<GameObject[]>();
    private GameObject[] cardList;
    private PlayManager playManager;

    void Awake()
    {
        playManager = FindObjectOfType<PlayManager>();
        deck.GetComponentInChildren<Button>().onClick.AddListener(DrawCard);
        deck.SetActive(true);
    }

    void Start()
    {
        movementPanels = new GameObject[TutorialTurnSystem.players.Count];
        interactionPanels = new GameObject[TutorialTurnSystem.players.Count];
        for (int i = 0; i < TutorialTurnSystem.players.Count; i++)
        {
            movementPanels[i] = Instantiate(movementCardPanel, parentCardPanels);
            cardList = new GameObject[MAXCARDS];
            for (int j = 0; j < MAXCARDS - 1; j++)
            {
                cardList[j] = Instantiate(card.gameObject, movementPanels[i].transform);
                cardList[j].GetComponent<NetworkCard>().SetRandomMovementCard();
            }
            cardList[MAXCARDS - 1] = Instantiate(card.gameObject, movementPanels[i].transform);
            cardList[MAXCARDS - 1].GetComponent<NetworkCard>().SetOriginalImage();
            movementHand.Add(cardList);

            interactionPanels[i] = Instantiate(interactionCardPanel, parentCardPanels);
            cardList = new GameObject[MAXCARDS];
            for (int j = 0; j < MAXCARDS - 1; j++)
            {
                cardList[j] = Instantiate(card.gameObject, interactionPanels[i].transform);
                cardList[j].GetComponent<NetworkCard>().SetRandomInteractionCard();
            }
            cardList[MAXCARDS - 1] = Instantiate(card.gameObject, interactionPanels[i].transform);
            cardList[MAXCARDS - 1].GetComponent<NetworkCard>().SetOriginalImage();
            interactionHand.Add(cardList);
        }
    }

    void Update()
    {
        if (!MyGameManager.pause)
        {
            if (!playManager.IsMiniGameRunning)
            {
                for (int i = 0; i < TutorialTurnSystem.players.Count; i++)
                {
                    if (playManager.PlayerTurnIndex == i)
                    {
                        if (!drawnCard)
                        {
                            interactionPanels[i].SetActive(true);
                            typeCardText.text = "Interaction Cards";
                        }
                        else
                        {
                            if (!finishInteraction)
                            {
                                interactionPanels[i].SetActive(true);
                                movementPanels[i].SetActive(false);
                                for (int j = 0; j < MAXCARDS && !interacting; j++)
                                {
                                    if (interactionHand[playManager.PlayerTurnIndex][j].GetComponent<NetworkCard>().Selected)
                                    {
                                        if (playManager.PlayerMoving ||
                                            playManager.IsMiniGameRunning ||
                                            interactionHand[playManager.PlayerTurnIndex][j].GetComponent<NetworkCard>().Empty)
                                        {
                                            interactionHand[playManager.PlayerTurnIndex][j].GetComponent<NetworkCard>().Selected = false;
                                        }
                                        else
                                        {
                                            //Disable interacting first (Order matters here)
                                            interacting = true;
                                            cardSelectedPosition = j;
                                            playManager.InteractPlayer(interactionHand[playManager.PlayerTurnIndex][j].GetComponent<NetworkCard>().Index, j);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                interactionPanels[i].SetActive(false);
                                movementPanels[i].SetActive(true);
                                typeCardText.text = "Movement Cards";
                                for (int j = MAXCARDS - 1; j >= 0; j--)
                                {
                                    if (movementHand[playManager.PlayerTurnIndex][j].GetComponent<NetworkCard>().Selected)
                                    {
                                        if (playManager.PlayerMoving ||
                                            playManager.IsMiniGameRunning ||
                                            movementHand[playManager.PlayerTurnIndex][j].GetComponent<NetworkCard>().Empty)
                                        {
                                            movementHand[playManager.PlayerTurnIndex][j].GetComponent<NetworkCard>().Selected = false;
                                        }
                                        else
                                        {
                                            playManager.MovePlayer(movementHand[playManager.PlayerTurnIndex][j].GetComponent<NetworkCard>().Index);
                                            movementHand[playManager.PlayerTurnIndex][j].GetComponent<NetworkCard>().SetOriginalImage();
                                            finishInteraction = false;
                                            drawnCard = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //Disable the the display of the other players cards
                        interactionPanels[i].SetActive(false);
                        movementPanels[i].SetActive(false);
                    }
                }
            }
        }
    }

    public void DeselectCard()
    {
        interactionHand[playManager.PlayerTurnIndex][cardSelectedPosition].GetComponent<NetworkCard>().Selected = false;
    }

    public void RemoveCard()
    {
        interactionHand[playManager.PlayerTurnIndex][cardSelectedPosition].GetComponent<NetworkCard>().SetOriginalImage();
    }

    public void ResetCard(int originalIndex)
    {
        interactionHand[playManager.PlayerTurnIndex][cardSelectedPosition].GetComponent<NetworkCard>().SetCard(originalIndex);
    }

    public void DrawCard()
    {
        for (int i = 0; i < MAXCARDS; i++)
        {
            if (interactionHand[playManager.PlayerTurnIndex][i].GetComponent<NetworkCard>().Empty)
            {
                interactionHand[playManager.PlayerTurnIndex][i].GetComponent<NetworkCard>().SetRandomInteractionCard();
                i = MAXCARDS;
            }
        }
        for (int i = 0; i < MAXCARDS; i++)
        {
            if (movementHand[playManager.PlayerTurnIndex][i].GetComponent<NetworkCard>().Empty)
            {
                movementHand[playManager.PlayerTurnIndex][i].GetComponent<NetworkCard>().SetRandomMovementCard();
                i = MAXCARDS;
            }
        }
        drawnCard = true;
        deck.SetActive(false);
    }

    public void ActionDrawCard()
    {
        int emptyCount = 0;
        for (int i = 0; i < MAXCARDS && emptyCount != 2; i++)
        {
            if (interactionHand[playManager.PlayerTurnIndex][i].GetComponent<NetworkCard>().Empty)
            {
                interactionHand[playManager.PlayerTurnIndex][i].GetComponent<NetworkCard>().SetRandomInteractionCard();
                emptyCount++;
            }
        }
    }

    public void DoAction(int cardType, int cardIndex, int playerIndex, int originalCardIndex, int swapCardType, int swapPosition)
    {
        switch (cardType)
        {
            case (int)NetworkCard.CardIndex.DISCARDCARD:
                interactionHand[playerIndex][cardIndex].GetComponent<NetworkCard>().SetOriginalImage();
                break;
            case (int)NetworkCard.CardIndex.SWITCHCARD:
                interactionHand[playManager.PlayerTurnIndex][originalCardIndex].GetComponent<NetworkCard>().SetOriginalImage();
                interactionHand[playManager.PlayerTurnIndex][swapPosition].GetComponent<NetworkCard>().SetCard(interactionHand[playerIndex][cardIndex].GetComponent<NetworkCard>().Index);
                interactionHand[playerIndex][cardIndex].GetComponent<NetworkCard>().SetCard(swapCardType);
                break;
            case (int)NetworkCard.CardIndex.STEALCARD:
                interactionHand[playManager.PlayerTurnIndex][originalCardIndex].GetComponent<NetworkCard>().SetCard(interactionHand[playerIndex][cardIndex].GetComponent<NetworkCard>().Index);
                interactionHand[playerIndex][cardIndex].GetComponent<NetworkCard>().SetOriginalImage();
                break;
        }
        playManager.InteractingWithPlayer = false;
    }

    //Returns the card positions of playerIndex hand
    //excludeCard allows you to exclude a card from selection
    public int[] GetCardPositionIndex(int playerIndex, int excludeCard = -1)
    {
        int size = GetNumberCards(playerIndex);
        if (excludeCard != -1) size -= 1;
        int[] indexes = new int[size];
        int count = 0;
        int i = 0;

        foreach (GameObject obj in interactionHand[playerIndex])
        {
            if (!obj.GetComponent<NetworkCard>().Empty &&
                obj.GetComponent<NetworkCard>().Index != excludeCard)
            {
                indexes[i] = count;
                i++;
            }
            count++;
        }

        return indexes;
    }

    //Return the amount of cards playerIndex has
    public int GetNumberCards(int playerIndex)
    {
        int count = 0;
        foreach (GameObject obj in interactionHand[playerIndex])
        {
            if (!obj.GetComponent<NetworkCard>().Empty)
            {
                count++;
            }
        }
        return count;
    }

    //Getting the indexes of a playerIndex hand
    //excludeCard allows you to exclude a card from selection
    public int[] GetPlayersHand(int playerIndex, int excludeCard = -1)
    {
        int size = GetNumberCards(playerIndex);
        if (excludeCard != -1) size -= 1;
        int[] indexes = new int[size];
        int i = 0;

        foreach (GameObject obj in interactionHand[playerIndex])
        {
            if (!obj.GetComponent<NetworkCard>().Empty &&
                obj.GetComponent<NetworkCard>().Index != excludeCard)
            {
                indexes[i] = obj.GetComponent<NetworkCard>().Index;
                i++;
            }
        }
        return indexes;
    }

    public bool FinishInteraction
    {
        get
        {
            return finishInteraction;
        }
        set
        {
            finishInteraction = value;
        }
    }


    public bool Interacting
    {
        get
        {
            return interacting;
        }
        set
        {
            interacting = value;
        }
    }

    public bool DrawnCard
    {
        get
        {
            return drawnCard;
        }
        set
        {
            drawnCard = value;
        }
    }
}
